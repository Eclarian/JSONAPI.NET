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
    public class ManagedModelSingleResourcePayloadBuilderTests
    {
        class Country
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public Continent Continent { get; set; }

            public ICollection<City> Cities { get; set; } 
        }

        class City
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        class Continent
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public void Returns_correct_payload_for_resource()
        {
            // Arrange
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Continent))).Returns(typeof(Continent).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Country))).Returns("countries");
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Continent))).Returns("continents");

            var nameProperty = new FieldModelProperty(typeof (Country).GetProperty("Name"), "name", false);
            var citiesProperty =
                new ToManyRelationshipModelProperty(typeof (Country).GetProperty("Cities"), "cities", false, typeof (City), null, null);
            var continentProperty =
                new ToOneRelationshipModelProperty(typeof (Country).GetProperty("Continent"), "continent", false, typeof (Continent), null, null);
            mockModelManager
                .Setup(m => m.GetProperties(typeof (Country)))
                .Returns(() => new ModelProperty[] { nameProperty, citiesProperty, continentProperty });
            
            var city1 = new City
            {
                Id = "10",
                Name = "Madrid"
            };

            var city2 = new City
            {
                Id = "11",
                Name = "Barcelona"
            };

            var continent = new Continent
            {
                Id = "1",
                Name = "Europe"
            };

            var country = new Country
            {
                Id = "4",
                Name = "Spain",
                Continent = continent,
                Cities = new List<City> { city1, city2 }
            };

            var mockLinkConventions = new Mock<ILinkConventions>(MockBehavior.Strict);
            mockLinkConventions.Setup(c => c.GetRelationshipLink(country, mockModelManager.Object, citiesProperty))
                .Returns(new Link("MOCK_CITIES_RELATIONSHIP_LINK", null));
            mockLinkConventions.Setup(c => c.GetRelatedResourceLink(country, mockModelManager.Object, citiesProperty))
                .Returns(new Link("MOCK_CITIES_RELATED_RESOURCE_LINK", null));
            mockLinkConventions.Setup(c => c.GetRelationshipLink(country, mockModelManager.Object, continentProperty))
                .Returns(new Link("MOCK_CONTINENT_RELATIONSHIP_LINK", null));
            mockLinkConventions.Setup(c => c.GetRelatedResourceLink(country, mockModelManager.Object, continentProperty))
                .Returns(new Link("MOCK_CONTINENT_RELATED_RESOURCE_LINK", null));

            // Act
            var payloadBuilder = new ManagedModelSingleResourcePayloadBuilder(mockModelManager.Object, mockLinkConventions.Object);
            var payload = payloadBuilder.BuildPayload(country);

            // Assert
            payload.PrimaryData.Id.Should().Be("4");
            payload.PrimaryData.Type.Should().Be("countries");
            ((string) payload.PrimaryData.Attributes["name"]).Should().Be("Spain");
            payload.PrimaryData.Relationships.Count.Should().Be(2);

            var citiesRelationship = payload.PrimaryData.Relationships.First();
            citiesRelationship.Key.Should().Be("cities");
            citiesRelationship.Value.SelfLink.Href.Should().Be("MOCK_CITIES_RELATIONSHIP_LINK");
            citiesRelationship.Value.RelatedResourceLink.Href.Should().Be("MOCK_CITIES_RELATED_RESOURCE_LINK");
            citiesRelationship.Value.Linkage.Should().BeNull();

            var continentRelationship = payload.PrimaryData.Relationships.Skip(1).First();
            continentRelationship.Key.Should().Be("continent");
            continentRelationship.Value.SelfLink.Href.Should().Be("MOCK_CONTINENT_RELATIONSHIP_LINK");
            continentRelationship.Value.RelatedResourceLink.Href.Should().Be("MOCK_CONTINENT_RELATED_RESOURCE_LINK");
            continentRelationship.Value.Linkage.Should().BeOfType<ToOneResourceLinkage>();
            ((string) continentRelationship.Value.Linkage.LinkageToken["type"]).Should().Be("continents");
            ((string) continentRelationship.Value.Linkage.LinkageToken["id"]).Should().Be("1");
        }
    }
}
