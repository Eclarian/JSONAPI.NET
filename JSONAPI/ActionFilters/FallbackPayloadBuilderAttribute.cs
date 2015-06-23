using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Converts ObjectContent to payload form if it isn't already
    /// </summary>
    public class FallbackPayloadBuilderAttribute : ActionFilterAttribute
    {
        private readonly IFallbackPayloadBuilder _fallbackPayloadBuilder;
        private readonly IErrorPayloadBuilder _errorPayloadBuilder;
        private readonly IErrorPayloadSerializer _errorPayloadSerializer;

        /// <summary>
        /// Creates a FallbackPayloadBuilderAttribute
        /// </summary>
        /// <param name="fallbackPayloadBuilder"></param>
        /// <param name="errorPayloadBuilder"></param>
        /// <param name="errorPayloadSerializer"></param>
        public FallbackPayloadBuilderAttribute(IFallbackPayloadBuilder fallbackPayloadBuilder,
            IErrorPayloadBuilder errorPayloadBuilder, IErrorPayloadSerializer errorPayloadSerializer)
        {
            _fallbackPayloadBuilder = fallbackPayloadBuilder;
            _errorPayloadBuilder = errorPayloadBuilder;
            _errorPayloadSerializer = errorPayloadSerializer;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext,
            CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Response != null)
            {
                var content = actionExecutedContext.Response.Content;
                var objectContent = content as ObjectContent;
                if (content != null && objectContent == null)
                    return;

                if (objectContent != null)
                {
                    // These payload types should be passed through; they are already ready to be serialized.
                    if (objectContent.Value is ISingleResourcePayload ||
                        objectContent.Value is IResourceCollectionPayload ||
                        objectContent.Value is IErrorPayload)
                        return;

                    var payloadValue = objectContent.Value;
                    if (actionExecutedContext.Exception != null)
                    {
                        payloadValue = _errorPayloadBuilder.BuildFromException(actionExecutedContext.Exception);
                    }
                    else
                    {
                        payloadValue = _fallbackPayloadBuilder.BuildPayload(payloadValue);
                    }

                    actionExecutedContext.Response.Content = new ObjectContent(payloadValue.GetType(), payloadValue, objectContent.Formatter);
                }
            }
            else if (actionExecutedContext.Exception != null)
            {
                var payload = _errorPayloadBuilder.BuildFromException(actionExecutedContext.Exception);
                var formatter = new JsonApiFormatter(null, null, _errorPayloadSerializer);
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new ObjectContent(payload.GetType(), payload, formatter)
                };
            }

            await Task.Yield();
        }
    }
}
