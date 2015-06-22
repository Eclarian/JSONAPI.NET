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
            mockModelManager.Setup(m => m.GetProperties(typeof (Country))).Returns(() => new ModelProperty[]
            {
                new FieldModelProperty(typeof(Country).GetProperty("Name"), "name", false),
                new RelationshipModelProperty(typeof(Country).GetProperty("Cities"), "cities", false, typeof(City), true),
                new RelationshipModelProperty(typeof(Country).GetProperty("Continent"), "continent", false, typeof(Continent), false)
            });
            var payloadBuilder = new ManagedModelSingleResourcePayloadBuilder(mockModelManager.Object);

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

            // Act
            var payload = payloadBuilder.BuildPayload(country);

            // Assert
            payload.PrimaryData.Id.Should().Be("4");
            payload.PrimaryData.Type.Should().Be("countries");
            ((string) payload.PrimaryData.Attributes["name"]).Should().Be("Spain");
            payload.PrimaryData.Relationships.Count.Should().Be(2);

            var citiesRelationship = payload.PrimaryData.Relationships.First();
            citiesRelationship.Key.Should().Be("cities");
            citiesRelationship.Value.SelfLink.Should().Be("http://example.com/countries/4/relationships/cities");
            citiesRelationship.Value.RelatedResourceLink.Should().Be("http://example.com/countries/4/cities");
            citiesRelationship.Value.Linkage.Should().BeNull();

            var continentRelationship = payload.PrimaryData.Relationships.Skip(1).First();
            continentRelationship.Key.Should().Be("continent");
            continentRelationship.Value.SelfLink.Should().Be("http://example.com/countries/4/relationships/continent");
            continentRelationship.Value.RelatedResourceLink.Should().Be("http://example.com/countries/4/continent");
            continentRelationship.Value.Linkage.Should().BeOfType<ToOneResourceLinkage>();
            ((string) continentRelationship.Value.Linkage.LinkageToken["type"]).Should().Be("continents");
            ((string) continentRelationship.Value.Linkage.LinkageToken["id"]).Should().Be("1");
        }
    }
}
