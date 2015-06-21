using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JSONAPI.Attributes;
using Newtonsoft.Json;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Post
    {
        [Key]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTimeOffset Created { get; set; }

        [JsonIgnore]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        [IncludeInPayload(true)]
        public virtual User Author { get; set; }

        [IncludeInPayload(true)]
        public virtual ICollection<Comment> Comments { get; set; }

        [IncludeInPayload(true)]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}