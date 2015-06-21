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
    public class SingleResourcePayloadSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_SingleResourcePayload_for_primary_data_only()
        {
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(It.IsAny<IResourceObject>(), It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder resource object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockResource = new Mock<IResourceObject>(MockBehavior.Strict);
            var payload = new SingleResourcePayload(mockResource.Object, null, null);

            var serializer = new SingleResourcePayloadSerializer(mockResourceObjectSerializer.Object, null);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_SingleResourcePayload_for_primary_data_only.json");
            mockResourceObjectSerializer.Verify(s => s.Serialize(mockResource.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_SingleResourcePayload_for_primary_data_and_metadata()
        {
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(It.IsAny<IResourceObject>(), It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder resource object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockResource = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var payload = new SingleResourcePayload(mockResource.Object, null, mockMetadata.Object);

            var serializer = new SingleResourcePayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_SingleResourcePayload_for_primary_data_and_metadata.json");
            mockResourceObjectSerializer.Verify(s => s.Serialize(mockResource.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_SingleResourcePayload_for_all_possible_members()
        {
            var mockPrimaryData = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource3 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(mockPrimaryData.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data object");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 1");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 2");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 3");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var relatedResources = new[] { relatedResource1.Object, relatedResource2.Object, relatedResource3.Object };
            var payload = new SingleResourcePayload(mockPrimaryData.Object, relatedResources, mockMetadata.Object);

            var serializer = new SingleResourcePayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Payload/Fixtures/Serialize_SingleResourcePayload_for_all_possible_members.json");
            mockResourceObjectSerializer.Verify(s => s.Serialize(mockPrimaryData.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectSerializer.Verify(s => s.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectSerializer.Verify(s => s.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectSerializer.Verify(s => s.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }
    }
}
