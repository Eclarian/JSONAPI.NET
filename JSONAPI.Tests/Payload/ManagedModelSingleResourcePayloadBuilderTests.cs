using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ManagedModelSingleResourcePayloadBuilderTests
    {
        class Country
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public void Returns_correct_payload_for_simple_resource()
        {
            // Arrange
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof (Country))).Returns(typeof (Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof (Country))).Returns("countries");
            var payloadBuilder = new ManagedModelSingleResourcePayloadBuilder(mockModelManager.Object);

            var model = new Country
            {
                Id = "4",
                Name = "Spain"
            };

            // Act
            var payload = payloadBuilder.BuildPayload(model);

            // Assert
            payload.PrimaryData.Id.Should().Be("4");
            payload.PrimaryData.Type.Should().Be("countries");
            ((string) payload.PrimaryData.Attributes["name"]).Should().Be("Spain");
        }
    }
}
