using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IMetadataSerializer
    /// </summary>
    public class MetadataSerializer : IMetadataSerializer
    {
        public Task Serialize(IMetadata metadata, JsonWriter writer)
        {
            if (metadata.MetaObject == null) throw new JsonSerializationException("The meta object cannot be null.");

            metadata.MetaObject.WriteTo(writer);
            return Task.FromResult(0);
        }
    }
}