using System;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class RelationshipObjectSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public void Serialize_relationship_with_no_required_fields()
        {
            var serializer = new RelationshipObjectSerializer(null, null, null);
            var resourceObject = new RelationshipObject(null, null, null);

            Func<Task> action = async () =>
            {
                await
                    GetSerializedString(serializer, resourceObject);
            };
            action.ShouldThrow<JsonSerializationException>()
                .WithMessage("At least one of `links`, `data`, or `meta` must be present in a relationship object.");
        }

        [TestMethod]
        public async Task Serialize_relationship_with_self_link_only()
        {
            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer
                .Setup(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some self link");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(mockLinkSerializer.Object, null, null);
            var resourceObject = new RelationshipObject(mockSelfLink.Object, null);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_self_link_only.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_relationship_with_related_link_only()
        {
            var mockRelatedLink = new Mock<ILink>(MockBehavior.Strict);

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer
                .Setup(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some related link");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(mockLinkSerializer.Object, null, null);
            var resourceObject = new RelationshipObject(null, mockRelatedLink.Object);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_related_link_only.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_relationship_with_self_link_and_related_link()
        {
            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);
            var mockRelatedLink = new Mock<ILink>(MockBehavior.Strict);

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer
                .Setup(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some self link");
                    return Task.FromResult(0);
                }).Verifiable();
            mockLinkSerializer
                .Setup(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some related link");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(mockLinkSerializer.Object, null, null);
            var resourceObject = new RelationshipObject( mockSelfLink.Object, mockRelatedLink.Object);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_self_link_and_related_link.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockLinkSerializer.Verify(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_relationship_with_linkage_only()
        {
            var mockLinkage = new Mock<IResourceLinkage>(MockBehavior.Strict);
            var mockLinkageSerializer = new Mock<IResourceLinkageSerializer>(MockBehavior.Strict);
            mockLinkageSerializer
                .Setup(s => s.Serialize(mockLinkage.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceLinkage metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("linkage goes here");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(null, mockLinkageSerializer.Object, null);
            var resourceObject = new RelationshipObject(mockLinkage.Object);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_linkage_only.json");
            mockLinkageSerializer.Verify(s => s.Serialize(mockLinkage.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_relationship_with_meta_only()
        {
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer
                .Setup(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(null, null, mockMetadataSerializer.Object);
            var resourceObject = new RelationshipObject(null, null, mockMetadata.Object);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_meta_only.json");
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_relationship_with_all_possible_members()
        {
            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);
            var mockRelatedLink = new Mock<ILink>(MockBehavior.Strict);

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer
                .Setup(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some self link");
                    return Task.FromResult(0);
                }).Verifiable();
            mockLinkSerializer
                .Setup(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("some related link");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockLinkage = new Mock<IResourceLinkage>(MockBehavior.Strict);
            var mockLinkageSerializer = new Mock<IResourceLinkageSerializer>(MockBehavior.Strict);
            mockLinkageSerializer
                .Setup(s => s.Serialize(mockLinkage.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceLinkage metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("linkage goes here");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer
                .Setup(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("metadata goes here");
                    return Task.FromResult(0);
                }).Verifiable();

            var serializer = new RelationshipObjectSerializer(mockLinkSerializer.Object, mockLinkageSerializer.Object, mockMetadataSerializer.Object);
            var resourceObject = new RelationshipObject(mockLinkage.Object, mockSelfLink.Object, mockRelatedLink.Object, mockMetadata.Object);

            await AssertSerializeOutput(serializer, resourceObject, "Payload/Fixtures/Serialize_relationship_with_all_possible_members.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockLinkSerializer.Verify(s => s.Serialize(mockRelatedLink.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockLinkageSerializer.Verify(s => s.Serialize(mockLinkage.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }
    }
}
