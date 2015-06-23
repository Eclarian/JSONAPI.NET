using System;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IErrorPayloadBuilder
    /// </summary>
    public class ErrorPayloadBuilder : IErrorPayloadBuilder
    {
        public IErrorPayload BuildFromException(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
