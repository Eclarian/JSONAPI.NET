using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class DefaultLinkConventionsTests
    {
        class Country
        {
            public string Id { get; set; }

            public ICollection<City> Cities { get; set; }
        }

        class City
        {
            public string Id { get; set; }
        }

        [TestMethod]
        public void GetRelationshipLink_returns_default_url_for_relationship()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof (Country).GetProperty("Cities"),
                "cities", false, typeof (City), null, null);

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/relationships/cities");
        }

        [TestMethod]
        public void GetRelatedResourceLink_returns_default_url_for_relationship()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, null);

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/cities");
        }
    }
}
