using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JSONAPI.Attributes;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        [IncludeInPayload(true)]
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}