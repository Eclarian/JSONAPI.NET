using System;
using JSONAPI.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ResourceTypeRegistryTests
    {
        private class InvalidModel // No Id discernable!
        {
            public string Data { get; set; }
        }

        private class CustomIdModel
        {
            [UseAsId]
            public Guid Uuid { get; set; }

            public string Data { get; set; }
        }

        private class DerivedPost : Post
        {
            
        }

        private class Band
        {
            [UseAsId]
            public string BandName { get; set; }

            [JsonProperty("THE-GENRE")]
            public string Genre { get; set; }
        }

        private class Salad
        {
            public string Id { get; set; }

            [JsonProperty("salad-type")]
            public string TheSaladType { get; set; }

            [JsonProperty("salad-type")]
            public string AnotherSaladType { get; set; }
        }

        private class Continent
        {
            [UseAsId]
            public string Name { get; set; }

            public string Id { get; set; }
        }

        private class Boat
        {
            public string Id { get; set; }

            public string Type { get; set; }
        }

        [TestMethod]
        public void Cant_register_type_with_missing_id()
        {
            // Arrange
            var mm = new ResourceTypeRegistry(new PluralizationService());

            // Act
            Action action = () => mm.RegisterResourceType(typeof(InvalidModel));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Unable to determine Id property for type `InvalidModel`.");
        }

        [TestMethod]
        public void Cant_register_type_with_non_id_property_called_id()
        {
            // Arrange
            var mm = new ResourceTypeRegistry(new PluralizationService());

            // Act
            Action action = () => mm.RegisterResourceType(typeof(Continent));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Continent` because it contains a non-id property that would serialize as \"id\".");
        }

        [TestMethod]
        public void Cant_register_type_with_property_called_type()
        {
            // Arrange
            var mm = new ResourceTypeRegistry(new PluralizationService());

            // Act
            Action action = () => mm.RegisterResourceType(typeof(Boat));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Boat` because it contains a property that would serialize as \"type\".");
        }

        [TestMethod]
        public void Cant_register_type_with_two_properties_with_the_same_name()
        {
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            Type saladType = typeof(Salad);

            // Act
            Action action = () => mm.RegisterResourceType(saladType);

            // Assert
            action.ShouldThrow<InvalidOperationException>().Which.Message.Should()
                .Be("Failed to register type `Salad` because contains multiple properties that would serialize as `salad-type`.");
        }

        [TestMethod]
        public void RegisterResourceType_sets_up_registration_correctly()
        {
            // Arrange
            var pluralizationService = new PluralizationService();

            // Act
            var registry = new ResourceTypeRegistry(pluralizationService);
            registry.RegisterResourceType(typeof(Post));
            var postReg = registry.GetRegistrationForType(typeof(Post));
            
            // Assert
            postReg.IdProperty.Should().BeSameAs(typeof(Post).GetProperty("Id"));
            postReg.ResourceTypeName.Should().Be("posts");
            postReg.Attributes.Length.Should().Be(1);
            postReg.Attributes.First().Property.Should().BeSameAs(typeof(Post).GetProperty("Title"));
            postReg.Relationships.Length.Should().Be(2);
            postReg.Relationships[0].IsToMany.Should().BeTrue();
            postReg.Relationships[0].Property.Should().BeSameAs(typeof(Post).GetProperty("Comments"));
            postReg.Relationships[0].SelfLinkTemplate.Should().Be("/posts/{1}/relationships/comments");
            postReg.Relationships[0].RelatedResourceLinkTemplate.Should().Be("/posts/{1}/comments");
            postReg.Relationships[1].IsToMany.Should().BeFalse();
            postReg.Relationships[1].Property.Should().BeSameAs(typeof(Post).GetProperty("Author"));
            postReg.Relationships[1].SelfLinkTemplate.Should().BeNull();
            postReg.Relationships[1].RelatedResourceLinkTemplate.Should().BeNull();
        }

        [TestMethod]
        public void GetRegistrationForType_returns_correct_value_for_registered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            mm.RegisterResourceType(typeof(Post));
            mm.RegisterResourceType(typeof(Author));
            mm.RegisterResourceType(typeof(Comment));
            mm.RegisterResourceType(typeof(UserGroup));

            // Act
            var postReg = mm.GetRegistrationForType(typeof(Post));
            var authorReg = mm.GetRegistrationForType(typeof(Author));
            var commentReg = mm.GetRegistrationForType(typeof(Comment));
            var userGroupReg = mm.GetRegistrationForType(typeof(UserGroup));

            // Assert
            postReg.ResourceTypeName.Should().Be("posts");
            authorReg.ResourceTypeName.Should().Be("authors");
            commentReg.ResourceTypeName.Should().Be("comments");
            userGroupReg.ResourceTypeName.Should().Be("user-groups");
        }

        [TestMethod]
        public void GetRegistrationForType_gets_registration_for_closest_registered_base_type_for_unregistered_type()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var registration = mm.GetRegistrationForType(typeof(DerivedPost));

            // Assert
            registration.Type.Should().Be(typeof(Post));
        }

        [TestMethod]
        public void GetRegistrationForType_fails_when_getting_unregistered_type()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);

            // Act
            Action action = () =>
            {
                mm.GetRegistrationForType(typeof(Post));
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No model registration was found for the type \"Post\".");
        }

        [TestMethod]
        public void GetRegistrationForResourceTypeName_fails_when_getting_unregistered_type_name()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);

            // Act
            Action action = () =>
            {
                mm.GetRegistrationForResourceTypeName("posts");
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No model registration was found for the type name \"posts\".");
        }

        [TestMethod]
        public void GetModelRegistrationForResourceTypeName_returns_correct_value_for_registered_names()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            mm.RegisterResourceType(typeof(Post));
            mm.RegisterResourceType(typeof(Author));
            mm.RegisterResourceType(typeof(Comment));
            mm.RegisterResourceType(typeof(UserGroup));

            // Act
            var postReg = mm.GetRegistrationForResourceTypeName("posts");
            var authorReg = mm.GetRegistrationForResourceTypeName("authors");
            var commentReg = mm.GetRegistrationForResourceTypeName("comments");
            var userGroupReg = mm.GetRegistrationForResourceTypeName("user-groups");

            // Assert
            postReg.Type.Should().Be(typeof (Post));
            authorReg.Type.Should().Be(typeof (Author));
            commentReg.Type.Should().Be(typeof (Comment));
            userGroupReg.Type.Should().Be(typeof (UserGroup));
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_type_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            mm.RegisterResourceType(typeof (Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof (Post));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_parent_type_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(DerivedPost));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_if_no_type_in_hierarchy_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(Comment));

            // Assert
            isRegistered.Should().BeFalse();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_for_collection_of_unregistered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(ICollection<Comment>));

            // Assert
            isRegistered.Should().BeFalse();
        }

        [TestMethod]
        public void GetJsonKeyForPropertyTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ResourceTypeRegistry(pluralizationService);

            // Act
            var idKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Id"));
            var nameKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Name"));
            var postsKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Posts"));

            // Assert
            Assert.AreEqual("id", idKey);
            Assert.AreEqual("name", nameKey);
            Assert.AreEqual("posts", postsKey);

        }
    }
}
