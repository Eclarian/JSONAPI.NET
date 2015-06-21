namespace JSONAPI.Payload
{
    /// <summary>
    /// Interface for JSON API payloads that represent a single resource
    /// </summary>
    public interface ISingleResourcePayload
    {
        /// <summary>
        /// The payload's primary data
        /// </summary>
        IResourceObject PrimaryData { get; }

        /// <summary>
        /// Data related to the primary data
        /// </summary>
        IResourceObject[] RelatedData { get; }

        /// <summary>
        /// Metadata for the payload as a whole
        /// </summary>
        IMetadata Metadata { get; }
    }
}