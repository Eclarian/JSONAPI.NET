using System;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Provides services for building an error payload
    /// </summary>
    public interface IErrorPayloadBuilder
    {
        /// <summary>
        /// Builds an error payload based on an exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        IErrorPayload BuildFromException(Exception exception);
    }
}