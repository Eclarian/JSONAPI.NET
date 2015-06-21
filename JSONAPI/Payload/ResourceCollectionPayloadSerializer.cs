using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IResourceCollectionPayloadSerializer
    /// </summary>
    public class ResourceCollectionPayloadSerializer : IResourceCollectionPayloadSerializer
    {
        private readonly IResourceObjectSerializer _resourceObjectSerializer;
        private readonly IMetadataSerializer _metadataSerializer;
        private const string PrimaryDataKeyName = "data";
        private const string RelatedDataKeyName = "included";
        private const string MetaKeyName = "meta";
        
        /// <summary>
        /// Creates a SingleResourcePayloadSerializer
        /// </summary>
        /// <param name="resourceObjectSerializer"></param>
        /// <param name="metadataSerializer"></param>
        public ResourceCollectionPayloadSerializer(IResourceObjectSerializer resourceObjectSerializer, IMetadataSerializer metadataSerializer)
        {
            _resourceObjectSerializer = resourceObjectSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IResourceCollectionPayload payload, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            writer.WriteStartArray();
            foreach (var resourceObject in payload.PrimaryData)
            {
                _resourceObjectSerializer.Serialize(resourceObject, writer);
            }
            writer.WriteEndArray();

            if (payload.RelatedData != null && payload.RelatedData.Any())
            {
                writer.WritePropertyName(RelatedDataKeyName);
                writer.WriteStartArray();
                foreach (var resourceObject in payload.RelatedData)
                {
                    _resourceObjectSerializer.Serialize(resourceObject, writer);
                }
                writer.WriteEndArray();
            }

            if (payload.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                _metadataSerializer.Serialize(payload.Metadata, writer);
            }

            writer.WriteEndObject();

            writer.Flush();

            return Task.FromResult(0);
        }
    }
}