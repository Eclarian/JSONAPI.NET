using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JSONAPI.Payload;

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
            SupportedMediaTypes.Insert(0, new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        public override bool CanReadType(Type t)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
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
                // TODO: use a payload builder to get a payload
                throw new NotImplementedException();
            }

            writer.Flush();

            return Task.FromResult(0);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            throw new NotImplementedException();
        }
    }
}
