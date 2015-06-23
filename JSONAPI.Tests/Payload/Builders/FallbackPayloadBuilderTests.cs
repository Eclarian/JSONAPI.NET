using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Payload.Builders
{
    [TestClass]
    public class FallbackPayloadBuilderTests
    {
        private const string GuidRegex = @"\b[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}\b";

        class Fruit
        {
            
        }

        [TestMethod]
        public void Creates_error_payload_for_unknown_types()
        {
            // Arrange
            var objectContent = new Fruit();

            var mockResourceTypeRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockResourceTypeRegistry.Setup(m => m.TypeIsRegistered(typeof (Fruit))).Returns(false);

            var mockPayload = new Mock<IErrorPayload>(MockBehavior.Strict);

            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            mockErrorPayloadBuilder
                .Setup(b => b.BuildFromException(It.IsAny<TypeRegistrationNotFoundException>()))
                .Returns(mockPayload.Object);

            var singleResourcePayloadBuilder = new Mock<ISingleResourcePayloadBuilder>(MockBehavior.Strict);
            var mockResourceCollectionPayloadBuilder = new Mock<IResourceCollectionPayloadBuilder>(MockBehavior.Strict);

            // Act
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(mockResourceTypeRegistry.Object, mockErrorPayloadBuilder.Object,
                singleResourcePayloadBuilder.Object, mockResourceCollectionPayloadBuilder.Object);
            var resultPayload = fallbackPayloadBuilder.BuildPayload(objectContent, null);

            // Assert
            resultPayload.Should().BeSameAs(mockPayload.Object);
        }
    }
}
