using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Interface responsible for serializing JSON API components
    /// </summary>
    /// <typeparam name="T">The type of component this service can serialize</typeparam>
    public interface IJsonApiSerializer<in T>
    {
        /// <summary>
        /// Serializes the given resource object
        /// </summary>
        /// <param name="component">The component to serialize</param>
        /// <param name="writer">The JSON writer to serialize with</param>
        /// <returns>A task that resolves when serialization is complete.</returns>
        Task Serialize(T component, JsonWriter writer);
    }
}