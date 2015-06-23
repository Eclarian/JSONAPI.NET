using System;
using JSONAPI.Core;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of ILinkConventions. Adheres to JSON API recommendations for URL formatting.
    /// </summary>
    public class DefaultLinkConventions : ILinkConventions
    {
        private readonly string _baseUrl;

        /// <summary>
        /// Creates a new DefaultLinkConventions
        /// </summary>
        /// <param name="baseUrl"></param>
        public DefaultLinkConventions(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public ILink GetRelationshipLink<TResource>(TResource relationshipOwner, IResourceTypeRegistry resourceTypeRegistry, ResourceTypeRelationship property)
        {
            var url = BuildRelationshipUrl(relationshipOwner, resourceTypeRegistry, property);
            var metadata = GetMetadataForRelationshipLink(relationshipOwner, property);
            return new Link(url, metadata);
        }

        public ILink GetRelatedResourceLink<TResource>(TResource relationshipOwner, IResourceTypeRegistry resourceTypeRegistry, ResourceTypeRelationship property)
        {
            var url = BuildRelatedResourceUrl(relationshipOwner, resourceTypeRegistry, property);
            var metadata = GetMetadataForRelatedResourceLink(relationshipOwner, property);
            return new Link(url, metadata);
        }

        private string GetSanitizedBaseUrl()
        {
            var sanitizedBaseUrl = _baseUrl;
            while (sanitizedBaseUrl[sanitizedBaseUrl.Length - 1] == '/')
                sanitizedBaseUrl = _baseUrl.Substring(0, _baseUrl.Length - 1);
            return sanitizedBaseUrl;
        }

        /// <summary>
        /// Constructs a URL for the relationship belonging to the given resource
        /// </summary>
        /// <param name="relationshipOwner"></param>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual string BuildRelationshipUrl(object relationshipOwner, IResourceTypeRegistry resourceTypeRegistry,
            ResourceTypeRelationship property)
        {
            var relationshipOwnerType = relationshipOwner.GetType();
            var sanitizedBaseUrl = GetSanitizedBaseUrl();
            var registration = resourceTypeRegistry.GetRegistrationForType(relationshipOwnerType);
            var id = registration.GetIdForResource(relationshipOwner);
            if (property.SelfLinkTemplate != null)
            {
                var replacedString = property.SelfLinkTemplate.Replace("{1}", id);
                return String.Format("{0}/{1}", sanitizedBaseUrl, replacedString);
            }

            return String.Format("{0}/{1}/{2}/relationships/{3}", sanitizedBaseUrl, registration.ResourceTypeName, id, property.JsonKey);
        }

        /// <summary>
        /// Gets a metadata object to serialize alongside the link URL for relationship links.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetMetadataForRelationshipLink<TResource>(TResource relationshipOwner, ResourceTypeRelationship property)
        {
            return null;
        }

        /// <summary>
        /// Constructs a URL for the resource(s) on the other side of the given relationship, belonging to the given resource
        /// </summary>
        /// <param name="relationshipOwner"></param>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual string BuildRelatedResourceUrl(object relationshipOwner, IResourceTypeRegistry resourceTypeRegistry,
            ResourceTypeRelationship property)
        {
            var relationshipOwnerType = relationshipOwner.GetType();
            var sanitizedBaseUrl = GetSanitizedBaseUrl();
            var registration = resourceTypeRegistry.GetRegistrationForType(relationshipOwnerType);
            var id = registration.GetIdForResource(relationshipOwner);
            if (property.RelatedResourceLinkTemplate != null)
            {
                var replacedString = property.RelatedResourceLinkTemplate.Replace("{1}", id);
                return String.Format("{0}/{1}", sanitizedBaseUrl, replacedString);
            }

            return String.Format("{0}/{1}/{2}/{3}", sanitizedBaseUrl, registration.ResourceTypeName, id, property.JsonKey);
        }

        /// <summary>
        /// Gets a metadata object to serialize alongside the link URL for related resource links.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetMetadataForRelatedResourceLink<TResource>(TResource relationshipOwner, ResourceTypeRelationship property)
        {
            return null;
        }
    }
}