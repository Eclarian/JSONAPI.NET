﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload.Builders
{
    [TestClass]
    public class RegistryDrivenSingleResourcePayloadBuilderTests
    {
        class Country
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public Continent Continent { get; set; }

            public ICollection<Province> Provinces { get; set; }

            public ICollection<City> Cities { get; set; } 
        }

        class Province
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public City Capital { get; set; }
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

            public ICollection<Province> Countries { get; set; }
        }

        [TestMethod]
        public void Returns_correct_payload_for_resource()
        {
            // Arrange
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

            var city3 = new City
            {
                Id = "12",
                Name = "Badajoz"
            };

            var province1 = new Province
            {
                Id = "506",
                Name = "Badajoz",
                Capital = city3
            };

            var province2 = new Province
            {
                Id = "507",
                Name = "Cuenca",
                Capital = null // Leaving null to test a null to-one
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
                Provinces = new List<Province> { province1, province2 },
                Cities = new List<City> { city1, city2, city3 }
            };


            // Country registration
            var countryName = new ResourceTypeAttribute(typeof(Country).GetProperty("Name"), "name");
            var countryCities =
                new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"), "cities", typeof(City), null, null);
            var countryProvinces =
                new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Provinces"), "provinces", typeof(Province), null, null);
            var countryContinent =
                new ToOneResourceTypeRelationship(typeof(Country).GetProperty("Continent"), "continent", typeof(Continent), null, null);
            var countryRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            countryRegistration.Setup(m => m.GetIdForResource(It.IsAny<Country>())).Returns((Country c) => country.Id);
            countryRegistration.Setup(m => m.ResourceTypeName).Returns("countries");
            countryRegistration.Setup(m => m.Attributes).Returns(new[] { countryName });
            countryRegistration
                .Setup(m => m.Relationships)
                .Returns(() => new ResourceTypeRelationship[] { countryCities, countryProvinces, countryContinent });


            // City registration
            var cityNameProperty = new ResourceTypeAttribute(typeof(City).GetProperty("Name"), "name");
            var cityRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            cityRegistration.Setup(m => m.ResourceTypeName).Returns("cities");
            cityRegistration.Setup(m => m.GetIdForResource(It.IsAny<City>())).Returns((City c) => c.Id);
            cityRegistration.Setup(m => m.Attributes).Returns(new[] { cityNameProperty });
            cityRegistration.Setup(m => m.Relationships).Returns(new ResourceTypeRelationship[] { });


            // Province registration
            var provinceName = new ResourceTypeAttribute(typeof(Province).GetProperty("Name"), "name");
            var provinceCapital = new ToOneResourceTypeRelationship(typeof(Province).GetProperty("Capital"), "capital", typeof(City), null, null);
            var provinceRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            provinceRegistration.Setup(m => m.ResourceTypeName).Returns("provinces");
            provinceRegistration.Setup(m => m.GetIdForResource(It.IsAny<Province>())).Returns((Province c) => c.Id);
            provinceRegistration.Setup(m => m.Attributes).Returns(new[] { provinceName });
            provinceRegistration
                .Setup(m => m.Relationships)
                .Returns(() => new ResourceTypeRelationship[] { provinceCapital });


            // Continent registration
            var continentName = new ResourceTypeAttribute(typeof(Continent).GetProperty("Name"), "name");
            var continentCountries =
                new ToManyResourceTypeRelationship(typeof(Continent).GetProperty("Countries"), "countries", typeof(Country), null, null);
            var continentRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            continentRegistration.Setup(m => m.ResourceTypeName).Returns("continents");
            continentRegistration.Setup(m => m.GetIdForResource(It.IsAny<Continent>())).Returns((Continent c) => c.Id);
            continentRegistration.Setup(m => m.Attributes).Returns(new[] { continentName });
            continentRegistration
                .Setup(m => m.Relationships)
                .Returns(() => new ResourceTypeRelationship[] { continentCountries });

            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(r => r.GetRegistrationForType(typeof(Country))).Returns(countryRegistration.Object);
            mockRegistry.Setup(r => r.GetRegistrationForType(typeof(City))).Returns(cityRegistration.Object);
            mockRegistry.Setup(r => r.GetRegistrationForType(typeof(Province))).Returns(provinceRegistration.Object);
            mockRegistry.Setup(r => r.GetRegistrationForType(typeof(Continent))).Returns(continentRegistration.Object);

            var linkConventions = new DefaultLinkConventions("http://www.example.com");

            // Act
            var payloadBuilder = new RegistryDrivenSingleResourcePayloadBuilder(mockRegistry.Object, linkConventions);
            var payload = payloadBuilder.BuildPayload(country, "provinces.capital", "continent");

            // Assert
            payload.PrimaryData.Id.Should().Be("4");
            payload.PrimaryData.Type.Should().Be("countries");
            ((string) payload.PrimaryData.Attributes["name"]).Should().Be("Spain");
            payload.PrimaryData.Relationships.Count.Should().Be(3);

            var citiesRelationship = payload.PrimaryData.Relationships.First();
            citiesRelationship.Key.Should().Be("cities");
            citiesRelationship.Value.SelfLink.Href.Should().Be("http://www.example.com/countries/4/relationships/cities");
            citiesRelationship.Value.RelatedResourceLink.Href.Should().Be("http://www.example.com/countries/4/cities");
            citiesRelationship.Value.Linkage.Should().BeNull();

            var provincesRelationship = payload.PrimaryData.Relationships.Skip(1).First();
            provincesRelationship.Key.Should().Be("provinces");
            provincesRelationship.Value.SelfLink.Href.Should().Be("http://www.example.com/countries/4/relationships/provinces");
            provincesRelationship.Value.RelatedResourceLink.Href.Should().Be("http://www.example.com/countries/4/provinces");
            provincesRelationship.Value.Linkage.Should().BeOfType<ToManyResourceLinkage>();
            provincesRelationship.Value.Linkage.LinkageToken.Should().BeOfType<JArray>();
            var provincesArray = (JArray) provincesRelationship.Value.Linkage.LinkageToken;
            ((string)provincesArray[0]["type"]).Should().Be("provinces");
            ((string)provincesArray[0]["id"]).Should().Be("506");
            ((string)provincesArray[1]["type"]).Should().Be("provinces");
            ((string)provincesArray[1]["id"]).Should().Be("507");

            var continentRelationship = payload.PrimaryData.Relationships.Skip(2).First();
            AssertToOneRelationship(continentRelationship, "continent",
                "http://www.example.com/countries/4/relationships/continent",
                "http://www.example.com/countries/4/continent",
                "continents", "1");

            payload.RelatedData.Length.Should().Be(4); // 2 provinces, 1 city, and 1 continent

            var province1RelatedData = payload.RelatedData[0];
            province1RelatedData.Id.Should().Be("506");
            province1RelatedData.Attributes["name"].Value<string>().Should().Be("Badajoz");
            province1RelatedData.Type.Should().Be("provinces");
            province1RelatedData.Relationships.Count.Should().Be(1);

            var province1CapitalRelationship = province1RelatedData.Relationships.First();
            AssertToOneRelationship(province1CapitalRelationship, "capital",
                "http://www.example.com/provinces/506/relationships/capital",
                "http://www.example.com/provinces/506/capital",
                "cities", "12");

            var province2RelatedData = payload.RelatedData[1];
            province2RelatedData.Id.Should().Be("507");
            province2RelatedData.Type.Should().Be("provinces");
            province2RelatedData.Attributes["name"].Value<string>().Should().Be("Cuenca");

            var province2CapitalRelationship = province2RelatedData.Relationships.First();
            AssertEmptyToOneRelationship(province2CapitalRelationship, "capital",
                "http://www.example.com/provinces/507/relationships/capital",
                "http://www.example.com/provinces/507/capital");

            var city3RelatedData = payload.RelatedData[2];
            city3RelatedData.Id.Should().Be("12");
            city3RelatedData.Type.Should().Be("cities");
            city3RelatedData.Attributes["name"].Value<string>().Should().Be("Badajoz");

            var continentRelatedData = payload.RelatedData[3];
            continentRelatedData.Id.Should().Be("1");
            continentRelatedData.Type.Should().Be("continents");
            continentRelatedData.Attributes["name"].Value<string>().Should().Be("Europe");
            continentRelatedData.Relationships.Count.Should().Be(1);
            var continentCountriesRelationship = continentRelatedData.Relationships.First();
            continentCountriesRelationship.Key.Should().Be("countries");
            continentCountriesRelationship.Value.SelfLink.Href.Should().Be("http://www.example.com/continents/1/relationships/countries");
            continentCountriesRelationship.Value.RelatedResourceLink.Href.Should().Be("http://www.example.com/continents/1/countries");
            continentCountriesRelationship.Value.Linkage.Should().BeNull();
        }

        private void AssertToOneRelationship(KeyValuePair<string, IRelationshipObject> relationshipPair, string keyName, string selfLink, string relatedResourceLink,
            string linkageType, string linkageId)
        {
            relationshipPair.Key.Should().Be(keyName);
            relationshipPair.Value.SelfLink.Href.Should().Be(selfLink);
            relationshipPair.Value.RelatedResourceLink.Href.Should().Be(relatedResourceLink);
            relationshipPair.Value.Linkage.Should().BeOfType<ToOneResourceLinkage>();
            ((string)relationshipPair.Value.Linkage.LinkageToken["type"]).Should().Be(linkageType);
            ((string)relationshipPair.Value.Linkage.LinkageToken["id"]).Should().Be(linkageId);
        }

        private void AssertEmptyToOneRelationship(KeyValuePair<string, IRelationshipObject> relationshipPair, string keyName, string selfLink, string relatedResourceLink)
        {
            relationshipPair.Key.Should().Be(keyName);
            relationshipPair.Value.SelfLink.Href.Should().Be(selfLink);
            relationshipPair.Value.RelatedResourceLink.Href.Should().Be(relatedResourceLink);
            relationshipPair.Value.Linkage.Should().BeNull();
        }
    }
}
