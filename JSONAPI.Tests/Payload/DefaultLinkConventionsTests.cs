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
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof (Country))).Returns(typeof (Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof (Country))).Returns("countries");

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/relationships/cities");
        }

        [TestMethod]
        public void GetRelationshipLink_returns_default_url_for_relationship_when_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, null);
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Country))).Returns("countries");

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com/");
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/relationships/cities");
        }

        [TestMethod]
        public void GetRelationshipLink_is_correct_if_template_is_present()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), "foo/{1}/bar", null);
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/foo/45/bar");
        }

        [TestMethod]
        public void GetRelationshipLink_is_correct_if_template_is_present_and_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), "foo/{1}/bar", null);
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com/");
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/foo/45/bar");
        }

        [TestMethod]
        public void GetRelatedResourceLink_returns_default_url_for_relationship()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, null);
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Country))).Returns("countries");

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/cities");
        }

        [TestMethod]
        public void GetRelatedResourceLink_returns_default_url_for_relationship_when_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, null);
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Country))).Returns("countries");

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com/");
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/cities");
        }

        [TestMethod]
        public void GetRelatedResourceLink_is_correct_if_template_is_present()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, "bar/{1}/qux");
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com");
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/bar/45/qux");
        }

        [TestMethod]
        public void GetRelatedResourceLink_is_correct_if_template_is_present_and_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyRelationshipModelProperty(typeof(Country).GetProperty("Cities"),
                "cities", false, typeof(City), null, "bar/{1}/qux");
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));

            // Act
            var conventions = new DefaultLinkConventions("https://www.example.com/");
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockModelManager.Object, relationshipProperty);

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/bar/45/qux");
        }

    }
}
