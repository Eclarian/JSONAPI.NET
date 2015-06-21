using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JSONAPI.Attributes;
using Newtonsoft.Json;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Comment
    {
        [Key]
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Created { get; set; }

        [Required]
        [JsonIgnore]
        public string PostId { get; set; }

        [Required]
        [JsonIgnore]
        public string AuthorId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("AuthorId")]
        [IncludeInPayload(true)]
        public virtual User Author { get; set; }
    }
}