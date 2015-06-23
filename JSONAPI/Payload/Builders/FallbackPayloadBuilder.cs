using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JSONAPI.Core;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Default implementation of IFallbackPayloadBuilder
    /// </summary>
    public class FallbackPayloadBuilder : IFallbackPayloadBuilder
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly IErrorPayloadBuilder _errorPayloadBuilder;
        private readonly ISingleResourcePayloadBuilder _singleResourcePayloadBuilder;
        private readonly IResourceCollectionPayloadBuilder _resourceCollectionPayloadBuilder;

        /// <summary>
        /// Creates a new FallbackPayloadBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="errorPayloadBuilder"></param>
        /// <param name="singleResourcePayloadBuilder"></param>
        /// <param name="resourceCollectionPayloadBuilder"></param>
        public FallbackPayloadBuilder(IResourceTypeRegistry resourceTypeRegistry, IErrorPayloadBuilder errorPayloadBuilder,
            ISingleResourcePayloadBuilder singleResourcePayloadBuilder,
            IResourceCollectionPayloadBuilder resourceCollectionPayloadBuilder)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
            _errorPayloadBuilder = errorPayloadBuilder;
            _singleResourcePayloadBuilder = singleResourcePayloadBuilder;
            _resourceCollectionPayloadBuilder = resourceCollectionPayloadBuilder;
        }

        public IJsonApiPayload BuildPayload(object obj, HttpRequestMessage requestMessage)
        {
            var type = obj.GetType();
            var isCollection = false;
            var enumerableElementType = GetEnumerableElementType(type);
            if (enumerableElementType != null)
            {
                type = enumerableElementType;
                isCollection = true;
            }

            try
            {
                if (!_resourceTypeRegistry.TypeIsRegistered(type))
                    throw new TypeRegistrationNotFoundException(type);
            }
            catch (TypeRegistrationNotFoundException ex)
            {
                return _errorPayloadBuilder.BuildFromException(ex);
            }

            if (isCollection)
            {
                throw new NotImplementedException();
            }
            else
            {
                return _singleResourcePayloadBuilder.BuildPayload(obj);
            }
        }

        private static Type GetEnumerableElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return collectionType.GetGenericArguments()[0];
            }

            var enumerableInterface = collectionType.GetInterface(typeof(IEnumerable<>).FullName);
            if (enumerableInterface == null) return null;

            var genericArguments = collectionType.GetGenericArguments();
            if (!genericArguments.Any()) return null;

            return genericArguments[0];
        }
    }
}
