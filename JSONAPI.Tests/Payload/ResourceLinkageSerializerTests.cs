using System.Threading.Tasks;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ResourceLinkageSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns("linkage goes here");

            var serializer = new ResourceLinkageSerializer();
            await AssertSerializeOutput(serializer, linkageObject.Object, "Payload/Fixtures/Serialize_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_null_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns((JToken)null);

            var serializer = new ResourceLinkageSerializer();
            await AssertSerializeOutput(serializer, linkageObject.Object, "Payload/Fixtures/Serialize_null_linkage.json");
        }
    }
}
