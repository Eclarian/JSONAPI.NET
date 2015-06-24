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
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registry.RegisterResourceType(typeof(InvalidModel));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Unable to determine Id property for type `InvalidModel`.");
        }

        [TestMethod]
        public void Cant_register_type_with_non_id_property_called_id()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registry.RegisterResourceType(typeof(Continent));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Continent` because it contains a non-id property that would serialize as \"id\".");
        }

        [TestMethod]
        public void Cant_register_type_with_property_called_type()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () => registry.RegisterResourceType(typeof(Boat));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Failed to register type `Boat` because it contains a property that would serialize as \"type\".");
        }

        [TestMethod]
        public void Cant_register_type_with_two_properties_with_the_same_name()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            Type saladType = typeof(Salad);

            // Act
            Action action = () => registry.RegisterResourceType(saladType);

            // Assert
            action.ShouldThrow<InvalidOperationException>().Which.Message.Should()
                .Be("Failed to register type `Salad` because contains multiple properties that would serialize as `salad-type`.");
        }

        [TestMethod]
        public void RegisterResourceType_sets_up_registration_correctly()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
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
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            registry.RegisterResourceType(typeof(Post));
            registry.RegisterResourceType(typeof(Author));
            registry.RegisterResourceType(typeof(Comment));
            registry.RegisterResourceType(typeof(UserGroup));

            // Act
            var postReg = registry.GetRegistrationForType(typeof(Post));
            var authorReg = registry.GetRegistrationForType(typeof(Author));
            var commentReg = registry.GetRegistrationForType(typeof(Comment));
            var userGroupReg = registry.GetRegistrationForType(typeof(UserGroup));

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
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            registry.RegisterResourceType(typeof(Post));

            // Act
            var registration = registry.GetRegistrationForType(typeof(DerivedPost));

            // Assert
            registration.Type.Should().Be(typeof(Post));
        }

        [TestMethod]
        public void GetRegistrationForType_fails_when_getting_unregistered_type()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () =>
            {
                registry.GetRegistrationForType(typeof(Post));
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No model registration was found for the type \"Post\".");
        }

        [TestMethod]
        public void GetRegistrationForResourceTypeName_fails_when_getting_unregistered_type_name()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            Action action = () =>
            {
                registry.GetRegistrationForResourceTypeName("posts");
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No model registration was found for the type name \"posts\".");
        }

        [TestMethod]
        public void GetModelRegistrationForResourceTypeName_returns_correct_value_for_registered_names()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            registry.RegisterResourceType(typeof(Post));
            registry.RegisterResourceType(typeof(Author));
            registry.RegisterResourceType(typeof(Comment));
            registry.RegisterResourceType(typeof(UserGroup));

            // Act
            var postReg = registry.GetRegistrationForResourceTypeName("posts");
            var authorReg = registry.GetRegistrationForResourceTypeName("authors");
            var commentReg = registry.GetRegistrationForResourceTypeName("comments");
            var userGroupReg = registry.GetRegistrationForResourceTypeName("user-groups");

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
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            registry.RegisterResourceType(typeof (Post));

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof (Post));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_parent_type_is_registered()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));
            registry.RegisterResourceType(typeof(Post));

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(DerivedPost));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_if_no_type_in_hierarchy_is_registered()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(Comment));

            // Assert
            isRegistered.Should().BeFalse();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_for_collection_of_unregistered_types()
        {
            // Arrange
            var registry = new ResourceTypeRegistry(new DefaultNamingConventions(new PluralizationService()));

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(ICollection<Comment>));

            // Assert
            isRegistered.Should().BeFalse();
        }
    }
}
