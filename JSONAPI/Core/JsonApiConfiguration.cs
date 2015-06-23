using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;
using JSONAPI.Payload;
using ErrorSerializer = JSONAPI.Payload.ErrorSerializer;

namespace JSONAPI.Core
{
    /// <summary>
    /// Configuration API for JSONAPI.NET
    /// </summary>
    public class JsonApiConfiguration
    {
        private readonly IModelManager _modelManager;
        private Func<IQueryablePayloadBuilder> _payloadBuilderFactory;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IModelManager modelManager)
        {
            if (modelManager == null) throw new Exception("You must provide a model manager to begin configuration.");

            _modelManager = modelManager;
            _payloadBuilderFactory = () => new DefaultQueryablePayloadBuilderConfiguration().GetBuilder(modelManager);
        }

        /// <summary>
        /// Allows configuring the default queryable payload builder
        /// </summary>
        /// <param name="configurationAction">Provides access to a fluent DefaultQueryablePayloadBuilderConfiguration object</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration UsingDefaultQueryablePayloadBuilder(Action<DefaultQueryablePayloadBuilderConfiguration> configurationAction)
        {
            _payloadBuilderFactory = () =>
            {
                var configuration = new DefaultQueryablePayloadBuilderConfiguration();
                configurationAction(configuration);
                return configuration.GetBuilder(_modelManager);
            };
            return this;
        }

        /// <summary>
        /// Allows overriding the default queryable payload builder
        /// </summary>
        /// <param name="queryablePayloadBuilder">The custom queryable payload builder to use</param>
        /// <returns></returns>
        public JsonApiConfiguration UseCustomQueryablePayloadBuilder(IQueryablePayloadBuilder queryablePayloadBuilder)
        {
            _payloadBuilderFactory = () => queryablePayloadBuilder;
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

            var queryablePayloadBuilder = _payloadBuilderFactory();
            var errorPayloadBuilder = new ErrorPayloadBuilder();
            httpConfig.Filters.Add(new FallbackPayloadBuilderAttribute(errorPayloadBuilder, errorPayloadSerializer));

            httpConfig.Services.Replace(typeof(IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
