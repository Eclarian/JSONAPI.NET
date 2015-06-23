using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// MediaTypeFormatter for JSON API
    /// </summary>
    public class JsonApiFormatter : JsonMediaTypeFormatter
    {
        private readonly ISingleResourcePayloadSerializer _singleResourcePayloadSerializer;
        private readonly IResourceCollectionPayloadSerializer _resourceCollectionPayloadSerializer;
        private readonly IErrorPayloadSerializer _errorPayloadSerializer;

        /// <summary>
        /// Creates a new JsonApiFormatter
        /// </summary>
        public JsonApiFormatter(ISingleResourcePayloadSerializer singleResourcePayloadSerializer,
            IResourceCollectionPayloadSerializer resourceCollectionPayloadSerializer,
            IErrorPayloadSerializer errorPayloadSerializer)
        {
            _singleResourcePayloadSerializer = singleResourcePayloadSerializer;
            _resourceCollectionPayloadSerializer = resourceCollectionPayloadSerializer;
            _errorPayloadSerializer = errorPayloadSerializer;

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        public override bool CanReadType(Type t)
        {
            return true;
        }

        public override bool CanWriteType(Type t)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            var writer = CreateJsonWriter(typeof(object), writeStream, effectiveEncoding);

            var singleResourcePayload = value as ISingleResourcePayload;
            var resourceCollectionPayload = value as IResourceCollectionPayload;
            var errorPayload = value as IErrorPayload;
            if (singleResourcePayload != null)
            {
                _singleResourcePayloadSerializer.Serialize(singleResourcePayload, writer);
            }
            else if (resourceCollectionPayload != null)
            {
                _resourceCollectionPayloadSerializer.Serialize(resourceCollectionPayload, writer);
            }
            else if (errorPayload != null)
            {
                _errorPayloadSerializer.Serialize(errorPayload, writer);
            }
            else
            {
                var error = value as HttpError;
                if (error != null)
                {
                    // The FallbackPayloadBuilderAttribute should have converted anything to one of the above payload types. If not, then there is
                    // a bug in this library. Render an error here.
                    WriteHttpError(error, writer);
                }
                else
                {
                    WriteErrorForUnsupportedType(value, writer);
                }
            }

            writer.Flush();

            return Task.FromResult(0);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            throw new NotImplementedException();
        }

        private void WriteHttpError(HttpError error, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("title");
            writer.WriteValue("Unexpected HttpError");
            writer.WritePropertyName("detail");
            writer.WriteValue("The JsonApiFormatter encountered an unexpected HttpError. If you have enabled FallbackPayloadBuilderAttribute, then this is most likely due to a bug in JSONAPI.NET.");
            writer.WritePropertyName("meta");
            writer.WriteStartObject();
            writer.WritePropertyName("httpErrorMessage");
            writer.WriteValue(error.Message);
            writer.WritePropertyName("exceptionMessage");
            writer.WriteValue(error.ExceptionMessage);
            writer.WritePropertyName("stackTrace");
            writer.WriteValue(error.StackTrace);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private void WriteErrorForUnsupportedType(object obj, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("title");
            writer.WriteValue("Unexpected Content");
            writer.WritePropertyName("detail");
            writer.WriteValue("The JsonApiFormatter was asked to serialize an unsupported object. If you have enabled FallbackPayloadBuilderAttribute, then this is most likely due to a bug in JSONAPI.NET.");
            writer.WritePropertyName("meta");
            writer.WriteStartObject();
            writer.WritePropertyName("objectTypeName");
            writer.WriteValue(obj.GetType().Name);
            writer.WriteEndObject();
            writer.WriteEndObject();
            
        }
    }
}
