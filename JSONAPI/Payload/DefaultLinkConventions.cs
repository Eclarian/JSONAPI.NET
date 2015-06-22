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

        public ILink GetRelationshipLink<TResource>(TResource relationshipOwner, RelationshipModelProperty property)
        {
            var url = BuildRelationshipUrl(relationshipOwner, property);
            var metadata = GetMetadataForRelationshipLink(relationshipOwner, property);
            return new Link(url, metadata);
        }

        public ILink GetRelatedResourceLink<TResource>(TResource relationshipOwner, RelationshipModelProperty property)
        {
            var url = BuildRelatedResourceUrl(relationshipOwner, property);
            var metadata = GetMetadataForRelatedResourceLink(relationshipOwner, property);
            return new Link(url, metadata);
        }

        /// <summary>
        /// Constructs a URL for the relationship belonging to the given resource
        /// </summary>
        /// <param name="relationshipOwner"></param>
        /// <param name="property"></param>
        /// <typeparam name="TResource"></typeparam>
        /// <returns></returns>
        protected virtual string BuildRelationshipUrl<TResource>(TResource relationshipOwner,
            RelationshipModelProperty property)
        {
            var strippedBaseUrl = _baseUrl;
            while (strippedBaseUrl[strippedBaseUrl.Length - 1] == '/')
                strippedBaseUrl = _baseUrl.Substring(0, _baseUrl.Length - 2);
            return String.Format("{0}/{1}/{2}/relationships/{3}", strippedBaseUrl, "countries", "45", property.JsonKey);
        }

        /// <summary>
        /// Gets a metadata object to serialize alongside the link URL for relationship links.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetMetadataForRelationshipLink<TResource>(TResource relationshipOwner, RelationshipModelProperty property)
        {
            return null;
        }

        /// <summary>
        /// Constructs a URL for the resource(s) on the other side of the given relationship, belonging to the given resource
        /// </summary>
        /// <param name="relationshipOwner"></param>
        /// <param name="property"></param>
        /// <typeparam name="TResource"></typeparam>
        /// <returns></returns>
        protected virtual string BuildRelatedResourceUrl<TResource>(TResource relationshipOwner,
            RelationshipModelProperty property)
        {
            var strippedBaseUrl = _baseUrl;
            while (strippedBaseUrl[strippedBaseUrl.Length - 1] == '/')
                strippedBaseUrl = _baseUrl.Substring(0, _baseUrl.Length - 2);
            return String.Format("{0}/{1}/{2}/{3}", strippedBaseUrl, "countries", "45", property.JsonKey);
        }

        /// <summary>
        /// Gets a metadata object to serialize alongside the link URL for related resource links.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetMetadataForRelatedResourceLink<TResource>(TResource relationshipOwner, RelationshipModelProperty property)
        {
            return null;
        }
    }
}