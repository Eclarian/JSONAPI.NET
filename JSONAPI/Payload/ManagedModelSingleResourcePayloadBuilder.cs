﻿using System.Collections.Generic;
using System.Linq;
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
        private readonly ILinkConventions _linkConventions;

        /// <summary>
        /// Creates a new ManagedModelSingleResourcePayloadBuilder
        /// </summary>
        /// <param name="modelManager">The model manager to use to locate model settings</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public ManagedModelSingleResourcePayloadBuilder(IModelManager modelManager, ILinkConventions linkConventions)
        {
            _modelManager = modelManager;
            _linkConventions = linkConventions;
        }

        public ISingleResourcePayload BuildPayload<TModel>(TModel primaryData)
        {
            var primaryDataType = primaryData.GetType();

            var idProp = _modelManager.GetIdProperty(primaryDataType);
            var resourceId = idProp.GetValue(primaryData).ToString();

            var typeName = _modelManager.GetResourceTypeNameForType(primaryDataType);

            var attributes = new Dictionary<string, JToken>();
            var relationships = new Dictionary<string, IRelationshipObject>();

            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();

            foreach (var modelProperty in _modelManager.GetProperties(primaryDataType))
            {
                if (modelProperty is FieldModelProperty)
                {
                    attributes[modelProperty.JsonKey] = JToken.FromObject(modelProperty.Property.GetValue(primaryData));
                    continue;
                }

                var relationshipModelProperty = modelProperty as RelationshipModelProperty;
                if (relationshipModelProperty != null)
                {
                    IResourceLinkage linkage;

                    if (relationshipModelProperty.IsToMany)
                    {
                        linkage = null;
                    }
                    else
                    {
                        var propertyValue = relationshipModelProperty.Property.GetValue(primaryData);
                        var propertyValueType = propertyValue.GetType();
                        var propertyValueTypeName = _modelManager.GetResourceTypeNameForType(propertyValueType);
                        var propertyValueTypeIdProperty = _modelManager.GetIdProperty(propertyValueType);
                        var propertyValueTypeId = propertyValueTypeIdProperty.GetValue(propertyValue).ToString();
                        var identifier = new ResourceIdentifier(propertyValueTypeName, propertyValueTypeId);
                        linkage = new ToOneResourceLinkage(identifier);
                    }

                    var selfLink = _linkConventions.GetRelationshipLink(primaryData, _modelManager, relationshipModelProperty);
                    var relatedResourceLink = _linkConventions.GetRelatedResourceLink(primaryData, _modelManager, relationshipModelProperty);

                    relationships[relationshipModelProperty.JsonKey] = new RelationshipObject(linkage, selfLink, relatedResourceLink);
                }
            }

            var primaryDataResource = new ResourceObject(typeName, resourceId, attributes, relationships);

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var payload = new SingleResourcePayload(primaryDataResource, relatedData, null);
            return payload;
        }
    }
}