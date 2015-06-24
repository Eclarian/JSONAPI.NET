using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using ErrorSerializer = JSONAPI.Payload.ErrorSerializer;

namespace JSONAPI.Core
{
    /// <summary>
    /// Configuration API for JSONAPI.NET
    /// </summary>
    public class JsonApiConfiguration
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly ILinkConventions _linkConventions;
        private Func<IResourceCollectionPayloadBuilder, IQueryableResourceCollectionPayloadBuilder> _queryablePayloadBuilderFactory;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
        {
            if (resourceTypeRegistry == null) throw new Exception("You must provide a model manager to begin configuration.");

            _resourceTypeRegistry = resourceTypeRegistry;
            _linkConventions = linkConventions;
            _queryablePayloadBuilderFactory = resourceCollectionPayloadBuilder =>
                new DefaultQueryablePayloadBuilderConfiguration().GetBuilder(resourceCollectionPayloadBuilder, _resourceTypeRegistry);
        }

        /// <summary>
        /// Allows configuring the default queryable payload builder
        /// </summary>
        /// <param name="configurationAction">Provides access to a fluent DefaultQueryablePayloadBuilderConfiguration object</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration UsingDefaultQueryablePayloadBuilder(Action<DefaultQueryablePayloadBuilderConfiguration> configurationAction)
        {
            _queryablePayloadBuilderFactory = resourceCollectionPayloadBuilder =>
            {
                var configuration = new DefaultQueryablePayloadBuilderConfiguration();
                configurationAction(configuration);
                return configuration.GetBuilder(resourceCollectionPayloadBuilder, _resourceTypeRegistry);
            };
            return this;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiConfiguration to</param>
        public void Apply(HttpConfiguration httpConfig)
        {
            var metadataSerializer = new MetadataSerializer();
            var linkSerializer = new LinkSerializer(metadataSerializer);
            var resourceLinkageSerializer = new ResourceLinkageSerializer();
            var relationshipObjectSerializer = new RelationshipObjectSerializer(linkSerializer, resourceLinkageSerializer, metadataSerializer);
            var resourceObjectSerializer = new ResourceObjectSerializer(relationshipObjectSerializer, linkSerializer, metadataSerializer);
            var errorSerializer = new ErrorSerializer(linkSerializer, metadataSerializer);
            var singleResourcePayloadSerializer = new SingleResourcePayloadSerializer(resourceObjectSerializer, metadataSerializer);
            var resourceCollectionPayloadSerializer = new ResourceCollectionPayloadSerializer(resourceObjectSerializer, metadataSerializer);
            var errorPayloadSerializer = new ErrorPayloadSerializer(errorSerializer, metadataSerializer);
            var formatter = new JsonApiFormatter(singleResourcePayloadSerializer, resourceCollectionPayloadSerializer, errorPayloadSerializer);

            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(formatter);

            var singleResourcePayloadBuilder = new RegistryDrivenSingleResourcePayloadBuilder(_resourceTypeRegistry, _linkConventions);
            var resourceCollectionPayloadBuilder = new RegistryDrivenResourceCollectionPayloadBuilder(_resourceTypeRegistry);
            var queryableResourcePayloadBuilder = _queryablePayloadBuilderFactory(resourceCollectionPayloadBuilder);
            var errorPayloadBuilder = new ErrorPayloadBuilder();
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(singleResourcePayloadBuilder, queryableResourcePayloadBuilder, resourceCollectionPayloadBuilder);
            httpConfig.Filters.Add(new FallbackPayloadBuilderAttribute(fallbackPayloadBuilder, errorPayloadBuilder, errorPayloadSerializer));

            httpConfig.Services.Replace(typeof(IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
