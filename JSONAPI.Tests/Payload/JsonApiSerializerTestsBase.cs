using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Payload
{
    public abstract class JsonApiSerializerTestsBase
    {
        protected async Task AssertSerializeOutput<TSerializer, TComponent>(TSerializer serializer, TComponent component, string expectedJsonFile)
            where TSerializer : IJsonApiSerializer<TComponent>
        {
            var output = await GetSerializedString(serializer, component);

            // Assert
            var expectedJson = TestHelpers.ReadEmbeddedFile(expectedJsonFile);
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            output.Should().Be(minifiedExpectedJson);
        }

        protected async Task<string> GetSerializedString<TSerializer, TComponent>(TSerializer serializer, TComponent component)
            where TSerializer : IJsonApiSerializer<TComponent>
        {
            using (var stream = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var writer = new JsonTextWriter(textWriter))
                    {
                        await serializer.Serialize(component, writer);
                        writer.Flush();
                        return Encoding.ASCII.GetString(stream.ToArray());
                    }
                }
            }
        }
    }
}
