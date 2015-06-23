using System.Collections.Generic;
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
    public class ManagedModelSingleResourcePayloadBuilderTests
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
            var mockModelManager = new Mock<IModelManager>(MockBehavior.Strict);
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Country))).Returns(typeof(Country).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetIdProperty(typeof(City))).Returns(typeof(City).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Province))).Returns(typeof(Province).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetIdProperty(typeof(Continent))).Returns(typeof(Continent).GetProperty("Id"));
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Country))).Returns("countries");
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(City))).Returns("cities");
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Province))).Returns("provinces");
            mockModelManager.Setup(m => m.GetResourceTypeNameForType(typeof(Continent))).Returns("continents");

            var countryNameProperty = new FieldModelProperty(typeof (Country).GetProperty("Name"), "name", false);
            var countryCitiesProperty =
                new ToManyRelationshipModelProperty(typeof (Country).GetProperty("Cities"), "cities", false, typeof (City), null, null);
            var countryProvincesProperty =
                new ToManyRelationshipModelProperty(typeof (Country).GetProperty("Provinces"), "provinces", false, typeof(Province), null, null);
            var countryContinentProperty =
                new ToOneRelationshipModelProperty(typeof(Country).GetProperty("Continent"), "continent", false, typeof(Continent), null, null);
            mockModelManager
                .Setup(m => m.GetProperties(typeof(Country)))
                .Returns(() => new ModelProperty[] { countryNameProperty, countryCitiesProperty, countryProvincesProperty, countryContinentProperty });

            var cityNameProperty = new FieldModelProperty(typeof(City).GetProperty("Name"), "name", false);
            mockModelManager
                .Setup(m => m.GetProperties(typeof(City)))
                .Returns(() => new ModelProperty[] { cityNameProperty });

            var provinceNameProperty = new FieldModelProperty(typeof(Province).GetProperty("Name"), "name", false);
            var provinceCapitalProperty = new ToOneRelationshipModelProperty(typeof (Province).GetProperty("Capital"), "capital", false, typeof(City), null, null);
            mockModelManager
                .Setup(m => m.GetProperties(typeof(Province)))
                .Returns(() => new ModelProperty[] { provinceNameProperty, provinceCapitalProperty });

            var continentNameProperty = new FieldModelProperty(typeof(Continent).GetProperty("Name"), "name", false);
            var continentCountriesProperty =
                new ToManyRelationshipModelProperty(typeof(Continent).GetProperty("Countries"), "countries", false, typeof(Country), null, null);
            mockModelManager
                .Setup(m => m.GetProperties(typeof (Continent)))
                .Returns(() => new ModelProperty[] { continentNameProperty, continentCountriesProperty });
            
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

            var linkConventions = new DefaultLinkConventions("http://www.example.com");

            // Act
            var payloadBuilder = new ManagedModelSingleResourcePayloadBuilder(mockModelManager.Object, linkConventions);
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

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_pathToInclude_equals_currentPath_with_one_segment()
        {
            // Arrange
            const string currentPath = "posts";
            const string pathToInclude = "posts";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_does_not_equal_or_start_with_currentPath()
        {
            // Arrange
            const string currentPath = "posts";
            const string pathToInclude = "comments";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_is_empty()
        {
            // Arrange
            const string currentPath = "";
            const string pathToInclude = "";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_is_null()
        {
            // Arrange
            const string currentPath = null;
            const string pathToInclude = null;

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_pathToInclude_equals_currentPath_with_multiple_segments()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.author";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_all_segments_of_currentPath_are_contained_by_pathToInclude()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.author.comments";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_all_segments_of_currentPath_are_contained_by_pathToInclude_but_start_doesnt_match()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "author.posts.author";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_starts_with_currentPath_but_segments_differ()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.authora";

            // Act
            var matches = ManagedModelSingleResourcePayloadBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }
    }
}
