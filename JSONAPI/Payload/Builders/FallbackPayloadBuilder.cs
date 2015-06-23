using System;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Default implementation of IFallbackPayloadBuilder
    /// </summary>
    public class FallbackPayloadBuilder : IFallbackPayloadBuilder
    {
        public object BuildPayload(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
