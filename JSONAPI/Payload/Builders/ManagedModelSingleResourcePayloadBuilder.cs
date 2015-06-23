using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload.Builders
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

        internal static bool PathExpressionMatchesCurrentPath(string currentPath, string pathToInclude)
        {
            if (string.IsNullOrEmpty(pathToInclude)) return false;
            if (currentPath == pathToInclude) return true;

            var currentPathSegments = currentPath.Split('.');
            var pathToIncludeSegments = pathToInclude.Split('.');

            // Same number of segments fails because we already checked for equality above
            if (currentPathSegments.Length >= pathToIncludeSegments.Length) return false;

            return !currentPathSegments.Where((t, i) => t != pathToIncludeSegments[i]).Any();
        }

        public ISingleResourcePayload BuildPayload<TModel>(TModel primaryData, params string[] includePathExpressions)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResource = CreateResourceObject(primaryData, idDictionariesByType, null, includePathExpressions);

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var payload = new SingleResourcePayload(primaryDataResource, relatedData, null);
            return payload;
        }

        private ResourceObject CreateResourceObject(object modelObject, IDictionary<string, IDictionary<string, ResourceObject>> idDictionariesByType,
            string currentPath, string[] includePathExpressions)
        {
            var modelObjectRuntimeType = modelObject.GetType();

            var attributes = new Dictionary<string, JToken>();
            var relationships = new Dictionary<string, IRelationshipObject>();
            
            foreach (var modelProperty in _modelManager.GetProperties(modelObjectRuntimeType))
            {
                if (modelProperty is FieldModelProperty)
                {
                    attributes[modelProperty.JsonKey] = JToken.FromObject(modelProperty.Property.GetValue(modelObject));
                    continue;
                }

                var relationshipModelProperty = modelProperty as RelationshipModelProperty;
                if (relationshipModelProperty != null)
                {
                    IResourceLinkage linkage = null;

                    var childPath = currentPath == null ? relationshipModelProperty.JsonKey : (currentPath + "." + relationshipModelProperty.JsonKey);
                    if (includePathExpressions != null &&
                        includePathExpressions.Any(e => PathExpressionMatchesCurrentPath(childPath, e)))
                    {
                        if (relationshipModelProperty.IsToMany)
                        {
                            var propertyValue =
                                (IEnumerable<object>) relationshipModelProperty.Property.GetValue(modelObject);

                            var identifiers = new List<IResourceIdentifier>();
                            foreach (var relatedResource in propertyValue)
                            {
                                var relatedResourceType = relatedResource.GetType();
                                var relatedResourceTypeName = _modelManager.GetResourceTypeNameForType(relatedResourceType);
                                var relatedResourceTypeIdProp = _modelManager.GetIdProperty(relatedResourceType);
                                var relatedResourceId = relatedResourceTypeIdProp.GetValue(relatedResource).ToString();
                                identifiers.Add(new ResourceIdentifier(relatedResourceTypeName, relatedResourceId));

                                IDictionary<string, ResourceObject> idDictionary;
                                if (!idDictionariesByType.TryGetValue(relatedResourceTypeName, out idDictionary))
                                {
                                    idDictionary = new Dictionary<string, ResourceObject>();
                                    idDictionariesByType[relatedResourceTypeName] = idDictionary;
                                }

                                ResourceObject relatedResourceObject;
                                if (!idDictionary.TryGetValue(relatedResourceId, out relatedResourceObject))
                                {
                                    relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType, childPath, includePathExpressions);
                                    idDictionary[relatedResourceId] = relatedResourceObject;
                                }
                            }
                            linkage = new ToManyResourceLinkage(identifiers.ToArray());
                        }
                        else
                        {
                            var relatedResource = relationshipModelProperty.Property.GetValue(modelObject);
                            if (relatedResource != null)
                            {
                                var relatedResourceType = relatedResource.GetType();
                                var relatedResourceTypeName =
                                    _modelManager.GetResourceTypeNameForType(relatedResourceType);
                                var relatedResourceTypeIdProp = _modelManager.GetIdProperty(relatedResourceType);
                                var relatedResourceId = relatedResourceTypeIdProp.GetValue(relatedResource).ToString();
                                var identifier = new ResourceIdentifier(relatedResourceTypeName, relatedResourceId);

                                IDictionary<string, ResourceObject> idDictionary;
                                if (!idDictionariesByType.TryGetValue(relatedResourceTypeName, out idDictionary))
                                {
                                    idDictionary = new Dictionary<string, ResourceObject>();
                                    idDictionariesByType[relatedResourceTypeName] = idDictionary;
                                }

                                ResourceObject relatedResourceObject;
                                if (!idDictionary.TryGetValue(relatedResourceId, out relatedResourceObject))
                                {
                                    relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType,
                                        childPath, includePathExpressions);
                                    idDictionary[relatedResourceId] = relatedResourceObject;
                                }

                                linkage = new ToOneResourceLinkage(identifier);
                            }
                        }
                    }

                    var selfLink = _linkConventions.GetRelationshipLink(modelObject, _modelManager, relationshipModelProperty);
                    var relatedResourceLink = _linkConventions.GetRelatedResourceLink(modelObject, _modelManager, relationshipModelProperty);

                    relationships[relationshipModelProperty.JsonKey] = new RelationshipObject(linkage, selfLink, relatedResourceLink);
                }
            }

            var idProp = _modelManager.GetIdProperty(modelObjectRuntimeType);
            var resourceId = idProp.GetValue(modelObject).ToString();
            var typeName = _modelManager.GetResourceTypeNameForType(modelObjectRuntimeType);
            return new ResourceObject(typeName, resourceId, attributes, relationships);
        }
    }
}