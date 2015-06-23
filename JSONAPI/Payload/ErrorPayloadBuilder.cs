﻿using System;
using System.Net;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IErrorPayloadBuilder
    /// </summary>
    public class ErrorPayloadBuilder : IErrorPayloadBuilder
    {
        public IErrorPayload BuildFromException(Exception exception)
        {
            var error = BuildErrorForException(exception);
            var topLevelMetadata = GetTopLevelMetadata();
            return new ErrorPayload(new [] { error }, topLevelMetadata);
        }

        /// <summary>
        /// Builds an error object for a given exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual IError BuildErrorForException(Exception exception)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Unhandled exception",
                Detail = "An unhandled exception was thrown while processing the request.",
                AboutLink = GetAboutLinkForException(exception),
                Status = HttpStatusCode.InternalServerError,
                Metadata = GetErrorMetadata(exception)
            };
            return error;
        }

        /// <summary>
        /// Gets metadata to serialize inside the error object for a given exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual IMetadata GetErrorMetadata(Exception exception)
        {
            return new ExceptionErrorMetadata(exception);
        }

        /// <summary>
        /// Gets a link to an about resource that yields further details about this particular occurrence of the problem.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual ILink GetAboutLinkForException(Exception exception)
        {
            return null;
        }

        /// <summary>
        /// Allows configuring top-level metadata for an error response payload.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetTopLevelMetadata()
        {
            return null;
        }
    }
}
