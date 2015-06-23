using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a payload for a resource that is registered with a resource type registry
    /// </summary>
    public class RegistryDrivenSingleResourcePayloadBuilder : ISingleResourcePayloadBuilder
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly ILinkConventions _linkConventions;

        /// <summary>
        /// Creates a new RegistryDrivenSingleResourcePayloadBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry">The resource type registry to use to locate the registered type</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public RegistryDrivenSingleResourcePayloadBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
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

        public ISingleResourcePayload BuildPayload(object primaryData, params string[] includePathExpressions)
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
            var resourceTypeRegistration = _resourceTypeRegistry.GetRegistrationForType(modelObjectRuntimeType);

            var attributes = new Dictionary<string, JToken>();
            var relationships = new Dictionary<string, IRelationshipObject>();

            foreach (var modelProperty in resourceTypeRegistration.Attributes)
            {
                attributes[modelProperty.JsonKey] = JToken.FromObject(modelProperty.Property.GetValue(modelObject));
            }

            foreach (var modelRelationship in resourceTypeRegistration.Relationships)
            {
                IResourceLinkage linkage = null;

                var childPath = currentPath == null
                    ? modelRelationship.JsonKey
                    : (currentPath + "." + modelRelationship.JsonKey);
                if (includePathExpressions != null &&
                    includePathExpressions.Any(e => PathExpressionMatchesCurrentPath(childPath, e)))
                {
                    if (modelRelationship.IsToMany)
                    {
                        var propertyValue =
                            (IEnumerable<object>) modelRelationship.Property.GetValue(modelObject);

                        var identifiers = new List<IResourceIdentifier>();
                        foreach (var relatedResource in propertyValue)
                        {
                            var identifier = GetResourceIdentifierForResource(relatedResource);
                            identifiers.Add(identifier);

                            IDictionary<string, ResourceObject> idDictionary;
                            if (!idDictionariesByType.TryGetValue(identifier.Type, out idDictionary))
                            {
                                idDictionary = new Dictionary<string, ResourceObject>();
                                idDictionariesByType[identifier.Type] = idDictionary;
                            }

                            ResourceObject relatedResourceObject;
                            if (!idDictionary.TryGetValue(identifier.Id, out relatedResourceObject))
                            {
                                relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType,
                                    childPath, includePathExpressions);
                                idDictionary[identifier.Id] = relatedResourceObject;
                            }
                        }
                        linkage = new ToManyResourceLinkage(identifiers.ToArray());
                    }
                    else
                    {
                        var relatedResource = modelRelationship.Property.GetValue(modelObject);
                        if (relatedResource != null)
                        {
                            var identifier = GetResourceIdentifierForResource(relatedResource);

                            IDictionary<string, ResourceObject> idDictionary;
                            if (!idDictionariesByType.TryGetValue(identifier.Type, out idDictionary))
                            {
                                idDictionary = new Dictionary<string, ResourceObject>();
                                idDictionariesByType[identifier.Type] = idDictionary;
                            }

                            ResourceObject relatedResourceObject;
                            if (!idDictionary.TryGetValue(identifier.Id, out relatedResourceObject))
                            {
                                relatedResourceObject = CreateResourceObject(relatedResource, idDictionariesByType,
                                    childPath, includePathExpressions);
                                idDictionary[identifier.Id] = relatedResourceObject;
                            }

                            linkage = new ToOneResourceLinkage(identifier);
                        }
                    }
                }

                var selfLink = _linkConventions.GetRelationshipLink(modelObject, _resourceTypeRegistry, modelRelationship);
                var relatedResourceLink = _linkConventions.GetRelatedResourceLink(modelObject, _resourceTypeRegistry, modelRelationship);

                relationships[modelRelationship.JsonKey] = new RelationshipObject(linkage, selfLink, relatedResourceLink);
            }

            var resourceId = resourceTypeRegistration.GetIdForResource(modelObject);
            return new ResourceObject(resourceTypeRegistration.ResourceTypeName, resourceId, attributes, relationships);
        }

        private ResourceIdentifier GetResourceIdentifierForResource(object resource)
        {
            var relatedResourceType = resource.GetType();
            var relatedResourceRegistration = _resourceTypeRegistry.GetRegistrationForType(relatedResourceType);
            var relatedResourceTypeName = relatedResourceRegistration.ResourceTypeName;
            var relatedResourceId = relatedResourceRegistration.GetIdForResource(resource);
            return new ResourceIdentifier(relatedResourceTypeName, relatedResourceId);
        }
    }
}