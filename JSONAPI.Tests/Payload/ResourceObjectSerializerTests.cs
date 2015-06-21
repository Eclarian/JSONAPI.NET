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
    public class ResourceObjectSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_without_attributes()
        {
            var resourceObject = new ResourceObject("countries", "1100");

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_without_attributes.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_attributes()
        {
            var attributes = new Dictionary<string, JToken>
            {
                { "name", "Triangle" },
                { "sides", 3 },
                { "foo", null }
            };
            var resourceObject = new ResourceObject("shapes", "1400", attributes);

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_attributes.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_relationships()
        {
            var mockCapital = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockNeighbors = new Mock<IRelationshipObject>(MockBehavior.Strict);

            var mockRelationshipObjectSerializer = new Mock<IRelationshipObjectSerializer>(MockBehavior.Strict);
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - capital");
                    return Task.FromResult(0);
                }).Verifiable();
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - neighbors");
                    return Task.FromResult(0);
                }).Verifiable();

            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", mockCapital.Object },
                { "neighbors", mockNeighbors.Object }
            };
            var resourceObject = new ResourceObject("states", "1400", relationships: relationships);

            var serializer = new ResourceObjectSerializer(mockRelationshipObjectSerializer.Object, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_relationships.json");
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_only_null_relationships()
        {
            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", null }
            };
            var resourceObject = new ResourceObject("states", "1400", relationships: relationships);

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_only_null_relationships.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_links()
        {
            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer.Setup(m => m.Serialize(It.IsAny<ILink>(), It.IsAny<JsonWriter>()))
                .Returns((ILink link, JsonWriter writer) =>
                {
                    writer.WriteValue("ILink placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);

            var resourceObject = new ResourceObject("states", "1400", selfLink: mockSelfLink.Object);

            var serializer = new ResourceObjectSerializer(null, mockLinkSerializer.Object, null);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_links.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_metadata()
        {
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var resourceObject = new ResourceObject("states", "1400", metadata: mockMetadata.Object);

            var serializer = new ResourceObjectSerializer(null, null, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_metadata.json");
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_all_possible_members()
        {
            var mockCapital = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockNeighbors = new Mock<IRelationshipObject>(MockBehavior.Strict);

            var mockRelationshipObjectSerializer = new Mock<IRelationshipObjectSerializer>(MockBehavior.Strict);
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - capital");
                    return Task.FromResult(0);
                }).Verifiable();
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - neighbors");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer.Setup(m => m.Serialize(It.IsAny<ILink>(), It.IsAny<JsonWriter>()))
                .Returns((ILink link, JsonWriter writer) =>
                {
                    writer.WriteValue("ILink placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();


            var attributes = new Dictionary<string, JToken>
            {
                { "name", "New York" },
                { "population", 19746227 },
                { "foo", null }
            };

            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", mockCapital.Object },
                { "neighbors", mockNeighbors.Object }
            };

            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);

            var resourceObject = new ResourceObject("states", "1400", attributes, relationships, mockSelfLink.Object, mockMetadata.Object);

            var serializer = new ResourceObjectSerializer(mockRelationshipObjectSerializer.Object, mockLinkSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_ResourceObject_for_resource_with_all_possible_members.json");
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }
    }
}
