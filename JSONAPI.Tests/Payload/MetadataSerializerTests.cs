using System;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class MetadataSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_metadata()
        {
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            mockMetadata.Setup(m => m.MetaObject)
                .Returns(() =>
                {
                    var subObject = new JObject();
                    subObject["color"] = "yellow";
                    subObject["foo"] = 3;

                    var obj = new JObject();
                    obj["banana"] = subObject;
                    obj["bar"] = new DateTime(1776, 07, 04);
                    return obj;
                });

            var serializer = new MetadataSerializer();
            await AssertSerializeOutput(serializer, mockMetadata.Object, "Payload/Fixtures/Serialize_metadata.json");
        }

        [TestMethod]
        public void Serialize_metadata_should_fail_if_object_is_null()
        {
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            mockMetadata.Setup(m => m.MetaObject)
                .Returns(() => null);

            var serializer = new MetadataSerializer();

            Func<Task> action = async () =>
            {
                await
                    GetSerializedString(serializer, mockMetadata.Object);
            };
            action.ShouldThrow<JsonSerializationException>()
                .WithMessage("The meta object cannot be null.");
        }
    }
}
