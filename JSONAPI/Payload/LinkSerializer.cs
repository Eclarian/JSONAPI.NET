﻿using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of ILinkSerializer
    /// </summary>
    public class LinkSerializer : ILinkSerializer
    {
        private readonly IMetadataSerializer _metadataSerializer;
        private const string HrefKeyName = "href";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Constructs a LinkSerializer
        /// </summary>
        /// <param name="metadataSerializer"></param>
        public LinkSerializer(IMetadataSerializer metadataSerializer)
        {
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(ILink link, JsonWriter writer)
        {
            if (link.Metadata == null)
            {
                writer.WriteValue(link.Href);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName(HrefKeyName);
                writer.WriteValue(link.Href);
                writer.WritePropertyName(MetaKeyName);
                _metadataSerializer.Serialize(link.Metadata, writer);
                writer.WriteEndObject();
            }
            return Task.FromResult(0);
        }
    }
}