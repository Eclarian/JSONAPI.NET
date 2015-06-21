using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Payload
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
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        ISingleResourcePayload BuildPayload<TModel>(TModel primaryData);
    }
}
