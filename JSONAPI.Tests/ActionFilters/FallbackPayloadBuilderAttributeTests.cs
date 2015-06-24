using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class FallbackPayloadBuilderAttributeTests
    {
        private HttpActionExecutedContext GetActionExecutedContext(object objectContentValue, Exception exception = null)
        {
            var mockMediaTypeFormatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            mockMediaTypeFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            mockMediaTypeFormatter.Setup(f => f.SetDefaultContentHeaders(It.IsAny<Type>(), It.IsAny<HttpContentHeaders>(), It.IsAny<MediaTypeHeaderValue>()));
            var response = new HttpResponseMessage
            {
                Content = new ObjectContent(objectContentValue.GetType(), objectContentValue, mockMediaTypeFormatter.Object)
            };
            var actionContext = new HttpActionContext { Response = response };
            return new HttpActionExecutedContext(actionContext, exception);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_ISingleResourcePayload_alone()
        {
            // Arrange
            var mockPayload = new Mock<ISingleResourcePayload>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IResourceCollectionPayload_alone()
        {
            // Arrange
            var mockPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IErrorPayload_alone()
        {
            // Arrange
            var mockPayload = new Mock<IErrorPayload>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
        }

        [TestMethod]
        public void OnActionExecutedAsync_creates_IErrorPayload_if_there_is_an_exception()
        {
            // Arrange
            var theException = new Exception("This is an error.");
            var actionExecutedContext = GetActionExecutedContext(new object(), theException);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var resultErrorPayload = new ErrorPayload(new IError[] {}, null);
            mockErrorPayloadBuilder.Setup(b => b.BuildFromException(theException)).Returns(resultErrorPayload);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            var newObjectContent = ((ObjectContent) actionExecutedContext.Response.Content).Value;
            newObjectContent.Should().BeSameAs(resultErrorPayload);
        }

        [TestMethod]
        public void OnActionExecutedAsync_creates_IErrorPayload_if_there_is_an_exception_with_no_response()
        {
            // Arrange
            var theException = new Exception("This is an error.");
            var actionContext = new HttpActionContext();
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, theException);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var resultErrorPayload = new ErrorPayload(new IError[] { }, null);
            mockErrorPayloadBuilder.Setup(b => b.BuildFromException(theException)).Returns(resultErrorPayload);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            var newObjectContent = ((ObjectContent)actionExecutedContext.Response.Content).Value;
            newObjectContent.Should().BeSameAs(resultErrorPayload);
        }

        private class Fruit
        {
        }

        [TestMethod]
        public void OnActionExecutedAsync_delegates_to_fallback_payload_builder_for_unknown_types()
        {
            // Arrange
            var payload = new Fruit();
            var actionExecutedContext = GetActionExecutedContext(payload);
            var cancellationTokenSource = new CancellationTokenSource();

            var mockResult = new Mock<IJsonApiPayload>(MockBehavior.Strict);
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            mockFallbackPayloadBuilder.Setup(b => b.BuildPayload(payload, It.IsAny<HttpRequestMessage>(), cancellationTokenSource.Token))
                .Returns(Task.FromResult(mockResult.Object));

            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent) actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
        }

        [TestMethod]
        public void OnActionExecutedAsync_creates_IErrorPayload_if_fallbackPayloadBuilder_throws_an_exception()
        {
            var payload = new Fruit();
            var actionExecutedContext = GetActionExecutedContext(payload);
            var cancellationTokenSource = new CancellationTokenSource();

            var exception = new Exception();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            mockFallbackPayloadBuilder
                .Setup(b => b.BuildPayload(payload, It.IsAny<HttpRequestMessage>(), cancellationTokenSource.Token))
                .Throws(exception);
            
            var mockResult = new Mock<IErrorPayload>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            mockErrorPayloadBuilder.Setup(b => b.BuildFromException(exception)).Returns(mockResult.Object);
            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object, mockErrorPayloadSerializer.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent) actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
        }
    }
}
