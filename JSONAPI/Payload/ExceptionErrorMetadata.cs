using System;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Metadata object for serializing exceptions in a response
    /// </summary>
    public class ExceptionErrorMetadata : IMetadata
    {
        /// <summary>
        /// Creates a new ExceptionErrorMetadata
        /// </summary>
        /// <param name="exception"></param>
        public ExceptionErrorMetadata(Exception exception)
        {
            MetaObject = new JObject();
            MetaObject["exceptionMessage"] = exception.Message;
            MetaObject["stackTrace"] = exception.StackTrace;
        }

        public JObject MetaObject { get; private set; }
    }
}