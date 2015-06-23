using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ErrorPayloadBuilderTests
    {
        private const string GuidRegex = @"\b[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}\b";

        [TestMethod]
        public void Builds_payload_from_exception()
        {
            // Arrange
            Exception theException;
            try
            {
                throw new Exception("This is the exception!");
            }
            catch (Exception ex)
            {
                theException = ex;
            }

            // Act
            var errorPayloadBuilder = new ErrorPayloadBuilder();
            var payload = errorPayloadBuilder.BuildFromException(theException);

            // Assert
            payload.Errors.Length.Should().Be(1);
            var error = payload.Errors.First();
            error.Id.Should().MatchRegex(GuidRegex);
            error.Title.Should().Be("Unhandled exception");
            error.Detail.Should().Be("An unhandled exception was thrown while processing the request.");
            error.Status.Should().Be(HttpStatusCode.InternalServerError);
            ((string) error.Metadata.MetaObject["exceptionMessage"]).Should().Be("This is the exception!");
        }
    }
}
