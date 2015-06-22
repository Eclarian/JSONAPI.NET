using System.Collections.Generic;
using JSONAPI.Attributes;

namespace JSONAPI.Tests.Models
{
    class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [RelationshipLinkTemplate("/posts/{1}/relationships/comments")]
        [RelatedResourceLinkTemplate("/posts/{1}/comments")]
        public IList<Comment> Comments { get; set; }
        public Author Author { get; set; }
    }
}
