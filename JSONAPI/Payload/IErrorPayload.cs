namespace JSONAPI.Payload
{
    /// <summary>
    /// Interface for JSON API payloads that represent a collection of errors
    /// </summary>
    public interface IErrorPayload
    {
        /// <summary>
        /// The errors to send in this payload
        /// </summary>
        IError[] Errors { get; }

        /// <summary>
        /// Metadata for the payload as a whole
        /// </summary>
        IMetadata Metadata { get; }
    }
}