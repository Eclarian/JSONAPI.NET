using System.Collections.Generic;
using JSONAPI.Core;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a payload for a list of models that are managed by a model manager
    /// </summary>
    public class RegistryDrivenResourceCollectionPayloadBuilder : IResourceCollectionPayloadBuilder
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        /// <summary>
        /// Creates a new ManagedModelResourceCollectionPayloadBuilder.
        /// </summary>
        /// <param name="resourceTypeRegistry"></param>
        public RegistryDrivenResourceCollectionPayloadBuilder(IResourceTypeRegistry resourceTypeRegistry)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
        }

        public IResourceCollectionPayload BuildPayload<TModel>(IEnumerable<TModel> primaryData, params string[] includePathExpressions)
        {
            throw new System.NotImplementedException();
        }
    }
}