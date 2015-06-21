using System.ComponentModel.DataAnnotations;
using JSONAPI.Attributes;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class City
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        [SerializeAs(SerializeAsOptions.RelatedLink)]
        [LinkTemplate("/cities/{1}/state")]
        [IncludeInPayload(true)]
        public virtual State State { get; set; }
    }
}