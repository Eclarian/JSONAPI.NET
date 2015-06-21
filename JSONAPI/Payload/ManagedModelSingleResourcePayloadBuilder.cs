using System;
using System.Collections.Generic;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Builds a payload for a model that is managed by a model manager
    /// </summary>
    public class ManagedModelSingleResourcePayloadBuilder : ISingleResourcePayloadBuilder
    {
        private readonly IModelManager _modelManager;

        /// <summary>
        /// Creates a new ManagedModelSingleResourcePayloadBuilder
        /// </summary>
        /// <param name="modelManager">The model manager to use to locate model settings</param>
        public ManagedModelSingleResourcePayloadBuilder(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        public ISingleResourcePayload BuildPayload<TModel>(TModel primaryData)
        {
            var primaryDataType = primaryData.GetType();

            var idProp = _modelManager.GetIdProperty(primaryDataType);
            var resourceId = idProp.GetValue(primaryData).ToString();

            var typeName = _modelManager.GetResourceTypeNameForType(primaryDataType);

            var attributes = new Dictionary<string, JToken>();


            var primaryDataResource = new ResourceObject(typeName, resourceId, attributes);

            var payload = new SingleResourcePayload(primaryDataResource, null, null);
            return payload;
        }
    }
}