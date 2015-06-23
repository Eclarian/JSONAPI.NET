namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Service to create a payload when the type is unknown at compile-time
    /// </summary>
    public interface IFallbackPayloadBuilder
    {
        /// <summary>
        /// Builds a JSON API payload based on the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        object BuildPayload(object obj);
    }
}