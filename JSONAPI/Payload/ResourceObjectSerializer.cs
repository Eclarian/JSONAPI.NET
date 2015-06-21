using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IResourceObjectSerializer
    /// </summary>
    public class ResourceObjectSerializer : IResourceObjectSerializer
    {
        private readonly IRelationshipObjectSerializer _relationshipObjectSerializer;
        private readonly ILinkSerializer _linkSerializer;
        private readonly IMetadataSerializer _metadataSerializer;
        private const string TypeKeyName = "type";
        private const string IdKeyName = "id";
        private const string AttributesKeyName = "attributes";
        private const string RelationshipsKeyName = "relationships";
        private const string MetaKeyName = "meta";
        private const string LinksKeyName = "links";
        private const string SelfLinkKeyName = "self";

        /// <summary>
        /// Constructs a new ResourceObjectSerializer
        /// </summary>
        /// <param name="relationshipObjectSerializer">The serializer to use for relationship objects</param>
        /// <param name="linkSerializer">The serializer to use for links</param>
        /// <param name="metadataSerializer">The serializer to use for metadata</param>
        public ResourceObjectSerializer(IRelationshipObjectSerializer relationshipObjectSerializer, ILinkSerializer linkSerializer, IMetadataSerializer metadataSerializer)
        {
            _relationshipObjectSerializer = relationshipObjectSerializer;
            _linkSerializer = linkSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IResourceObject resourceObject, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(TypeKeyName);
            writer.WriteValue(resourceObject.Type);
            writer.WritePropertyName(IdKeyName);
            writer.WriteValue(resourceObject.Id);

            if (resourceObject.Attributes != null && resourceObject.Attributes.Any())
            {
                writer.WritePropertyName(AttributesKeyName);
                writer.WriteStartObject();
                foreach (var attribute in resourceObject.Attributes)
                {
                    writer.WritePropertyName(attribute.Key);
                    if (attribute.Value == null)
                        writer.WriteNull();
                    else
                        attribute.Value.WriteTo(writer);
                }
                writer.WriteEndObject();
            }

            if (resourceObject.Relationships != null)
            {
                var relationshipsToRender = resourceObject.Relationships.Where(r => r.Value != null).ToArray();
                if (relationshipsToRender.Any())
                {
                    writer.WritePropertyName(RelationshipsKeyName);
                    writer.WriteStartObject();
                    foreach (var relationship in relationshipsToRender)
                    {
                        if (relationship.Value == null) continue;
                        writer.WritePropertyName(relationship.Key);
                        _relationshipObjectSerializer.Serialize(relationship.Value, writer);
                    }
                    writer.WriteEndObject();
                }
            }

            if (resourceObject.SelfLink != null)
            {
                writer.WritePropertyName(LinksKeyName);
                writer.WriteStartObject();
                writer.WritePropertyName(SelfLinkKeyName);
                _linkSerializer.Serialize(resourceObject.SelfLink, writer);
                writer.WriteEndObject();
            }

            if (resourceObject.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                _metadataSerializer.Serialize(resourceObject.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }
    }
}