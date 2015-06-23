namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a response payload from primary data objects
    /// </summary>
    public interface ISingleResourcePayloadBuilder
    {
        /// <summary>
        /// Builds an ISingleResourcePayload from the given model object
        /// </summary>
        /// <param name="primaryData"></param>
        /// <param name="includePathExpressions">A list of dot-separated paths to include in the compound document.
        /// If this collection is null or empty, no linkage will be included.</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        ISingleResourcePayload BuildPayload<TModel>(TModel primaryData, params string[] includePathExpressions);
    }
}
