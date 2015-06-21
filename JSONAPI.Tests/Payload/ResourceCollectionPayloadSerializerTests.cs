using System.Collections.Generic;
using System.Threading.Tasks;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ResourceCollectionPayloadSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_primary_data_only()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            var payload = new ResourceCollectionPayload(primaryData, null, null);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, null);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_ResourceCollectionPayload_for_primary_data_only.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_primary_data_only_and_metadata()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            var payload = new ResourceCollectionPayload(primaryData, null, mockMetadata.Object);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_ResourceCollectionPayload_for_primary_data_only_and_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_all_possible_members()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource3 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 3");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            var relatedResources = new[] { relatedResource1.Object, relatedResource2.Object, relatedResource3.Object };
            var payload = new ResourceCollectionPayload(primaryData, relatedResources, mockMetadata.Object);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_ResourceCollectionPayload_for_all_possible_members.json");
        }
    }
}
