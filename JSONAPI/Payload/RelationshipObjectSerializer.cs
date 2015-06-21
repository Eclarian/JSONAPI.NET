using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IRelationshipObjectSerializer
    /// </summary>
    public class RelationshipObjectSerializer : IRelationshipObjectSerializer
    {
        private const string LinksKeyName = "links";
        private const string SelfLinkKeyName = "self";
        private const string RelatedLinkKeyName = "related";
        private const string LinkageKeyName = "data";
        private const string MetaKeyName = "meta";

        private readonly ILinkSerializer _linkSerializer;
        private readonly IResourceLinkageSerializer _resourceLinkageSerializer;
        private readonly IMetadataSerializer _metadataSerializer;

        /// <summary>
        /// Creates a new RelationshipObjectSerializer
        /// </summary>
        public RelationshipObjectSerializer(ILinkSerializer linkSerializer, IResourceLinkageSerializer resourceLinkageSerializer, IMetadataSerializer metadataSerializer)
        {
            _linkSerializer = linkSerializer;
            _resourceLinkageSerializer = resourceLinkageSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IRelationshipObject relationshipObject, JsonWriter writer)
        {
            if (relationshipObject.Metadata == null && relationshipObject.SelfLink == null &&
                relationshipObject.RelatedResourceLink == null && relationshipObject.Linkage == null)
                throw new JsonSerializationException(
                    String.Format("At least one of `{0}`, `{1}`, or `{2}` must be present in a relationship object.",
                        LinksKeyName, LinkageKeyName, MetaKeyName));

            writer.WriteStartObject();

            if (relationshipObject.SelfLink != null || relationshipObject.RelatedResourceLink != null)
            {
                writer.WritePropertyName(LinksKeyName);
                writer.WriteStartObject();

                if (relationshipObject.SelfLink != null)
                {
                    writer.WritePropertyName(SelfLinkKeyName);
                    _linkSerializer.Serialize(relationshipObject.SelfLink, writer);
                }
                if (relationshipObject.RelatedResourceLink != null)
                {
                    writer.WritePropertyName(RelatedLinkKeyName);
                    _linkSerializer.Serialize(relationshipObject.RelatedResourceLink, writer);
                }

                writer.WriteEndObject();
            }

            if (relationshipObject.Linkage != null)
            {
                writer.WritePropertyName(LinkageKeyName);
                _resourceLinkageSerializer.Serialize(relationshipObject.Linkage, writer);
            }

            if (relationshipObject.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                _metadataSerializer.Serialize(relationshipObject.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }
    }
}