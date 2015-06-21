using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IResourceLinkageSerializer
    /// </summary>
    public class ResourceLinkageSerializer : IResourceLinkageSerializer
    {
        public Task Serialize(IResourceLinkage linkage, JsonWriter writer)
        {
            if (linkage.LinkageToken == null)
                writer.WriteNull();
            else
                linkage.LinkageToken.WriteTo(writer);
            return Task.FromResult(0);
        }
    }
}