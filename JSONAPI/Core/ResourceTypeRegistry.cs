using System.Collections.ObjectModel;
using JSONAPI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JSONAPI.Extensions;
using Newtonsoft.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Default implementation of IModelRegistry
    /// </summary>
    public class ResourceTypeRegistry : IResourceTypeRegistry
    {
        private readonly IPluralizationService _pluralizationService;

        /// <summary>
        /// Creates a new ModelManager
        /// </summary>
        /// <param name="pluralizationService"></param>
        public ResourceTypeRegistry(IPluralizationService pluralizationService)
        {
            _pluralizationService = pluralizationService;
            RegistrationsByName = new Dictionary<string, ResourceTypeRegistration>();
            RegistrationsByType = new Dictionary<Type, ResourceTypeRegistration>();
        }

        /// <summary>
        /// Represents a type's registration with a model manager
        /// </summary>
        protected sealed class ResourceTypeRegistration : IResourceTypeRegistration
        {
            private readonly IReadOnlyDictionary<string, ResourceTypeField> _fields;

            internal ResourceTypeRegistration(Type type, PropertyInfo idProperty, string resourceTypeName, IDictionary<string, ResourceTypeField> fields)
            {
                IdProperty = idProperty;
                Type = type;
                ResourceTypeName = resourceTypeName;
                Attributes = fields.Values.OfType<ResourceTypeAttribute>().ToArray();
                Relationships = fields.Values.OfType<ResourceTypeRelationship>().ToArray();
                _fields = new ReadOnlyDictionary<string, ResourceTypeField>(fields);
            }

            public Type Type { get; private set; }

            public PropertyInfo IdProperty { get; private set; }

            public string ResourceTypeName { get; private set; }

            public ResourceTypeAttribute[] Attributes { get; private set; }

            public ResourceTypeRelationship[] Relationships { get; private set; }

            public string GetIdForResource(object resource)
            {
                return IdProperty.GetValue(resource).ToString();
            }

            public ResourceTypeField GetFieldByName(string name)
            {
                return _fields.ContainsKey(name) ? _fields[name] : null;
            }
        }

        protected readonly IDictionary<string, ResourceTypeRegistration> RegistrationsByName;
        protected readonly IDictionary<Type, ResourceTypeRegistration> RegistrationsByType; 

        public bool TypeIsRegistered(Type type)
        {
            var registration = FindRegistrationForType(type);
            return registration != null;
        }

        public IResourceTypeRegistration GetRegistrationForType(Type type)
        {
            var reg = FindRegistrationForType(type);
            if (reg == null)
                throw new TypeRegistrationNotFoundException(type);

            return reg;
        }

        public IResourceTypeRegistration GetRegistrationForResourceTypeName(string resourceTypeName)
        {
            lock (RegistrationsByName)
            {
                ResourceTypeRegistration registration;
                if (!RegistrationsByName.TryGetValue(resourceTypeName, out registration))
                    throw new TypeRegistrationNotFoundException(resourceTypeName);

                return registration;
            }
        }

        private ResourceTypeRegistration FindRegistrationForType(Type type)
        {
            lock (RegistrationsByType)
            {
                var currentType = type;
                while (currentType != null && currentType != typeof(Object))
                {
                    ResourceTypeRegistration registration;
                    if (RegistrationsByType.TryGetValue(currentType, out registration))
                        return registration;

                    // This particular type wasn't registered, but maybe the base type was.
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Registers a type with this ModelManager.
        /// </summary>
        /// <param name="type">The type to register.</param>
        public ResourceTypeRegistry RegisterResourceType(Type type)
        {
            var resourceTypeName = CalculateResourceTypeNameForType(type);
            return RegisterResourceType(type, resourceTypeName);
        }

        /// <summary>
        /// Registeres a type with this ModelManager, using a default resource type name.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="resourceTypeName">The resource type name to use</param>
        public ResourceTypeRegistry RegisterResourceType(Type type, string resourceTypeName)
        {
            lock (RegistrationsByType)
            {
                lock (RegistrationsByName)
                {
                    if (RegistrationsByType.ContainsKey(type))
                        throw new InvalidOperationException(String.Format("The type `{0}` has already been registered.",
                            type.FullName));

                    if (RegistrationsByName.ContainsKey(resourceTypeName))
                        throw new InvalidOperationException(
                            String.Format("The resource type name `{0}` has already been registered.", resourceTypeName));

                    var fieldMap = new Dictionary<string, ResourceTypeField>();

                    var idProperty = CalculateIdProperty(type);
                    if (idProperty == null)
                        throw new InvalidOperationException(String.Format(
                            "Unable to determine Id property for type `{0}`.", type.Name));

                    var props = type.GetProperties();
                    foreach (var prop in props)
                    {
                        if (prop == idProperty) continue;

                        var ignore = prop.CustomAttributes.Any(c => c.AttributeType == typeof(JsonIgnoreAttribute));
                        if (ignore) continue;

                        var jsonKey = CalculateJsonKeyForProperty(prop);
                        if (jsonKey == "id")
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because it contains a non-id property that would serialize as \"id\".", type.Name));

                        if (jsonKey == "type")
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because it contains a property that would serialize as \"type\".", type.Name));

                        if (fieldMap.ContainsKey(jsonKey))
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because contains multiple properties that would serialize as `{1}`.",
                                    type.Name, jsonKey));
                        var property = CreateResourceTypeField(prop, jsonKey);
                        fieldMap[jsonKey] = property;
                    }

                    var registration = new ResourceTypeRegistration(type, idProperty, resourceTypeName, fieldMap);

                    RegistrationsByType.Add(type, registration);
                    RegistrationsByName.Add(resourceTypeName, registration);
                }
            }

            return this;
        }

        /// <summary>
        /// Creates a cacheable model field representation from a PropertyInfo
        /// </summary>
        /// <param name="prop">The property</param>
        /// <param name="jsonKey">The key that this model property will be serialized as</param>
        /// <returns>A model field represenation</returns>
        protected virtual ResourceTypeField CreateResourceTypeField(PropertyInfo prop, string jsonKey)
        {
            var type = prop.PropertyType;

            if (prop.PropertyType.CanWriteAsJsonApiAttribute())
                return new ResourceTypeAttribute(prop, jsonKey);

            var selfLinkTemplateAttribute = prop.GetCustomAttributes().OfType<RelationshipLinkTemplate>().FirstOrDefault();
            var selfLinkTemplate = selfLinkTemplateAttribute == null ? null : selfLinkTemplateAttribute.TemplateString;
            var relatedResourceLinkTemplateAttribute = prop.GetCustomAttributes().OfType<RelatedResourceLinkTemplate>().FirstOrDefault();
            var relatedResourceLinkTemplate = relatedResourceLinkTemplateAttribute == null ? null : relatedResourceLinkTemplateAttribute.TemplateString;

            var isToMany =
                type.IsArray ||
                (type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) && type.IsGenericType);

            if (!isToMany) return new ToOneResourceTypeRelationship(prop, jsonKey, type, selfLinkTemplate, relatedResourceLinkTemplate);
            var relatedType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
            return new ToManyResourceTypeRelationship(prop, jsonKey, relatedType, selfLinkTemplate, relatedResourceLinkTemplate);
        }

        /// <summary>
        /// Determines the resource type name for a given type.
        /// </summary>
        /// <param name="type">The type to calculate the resouce type name for</param>
        /// <returns>The type's resource type name</returns>
        protected virtual string CalculateResourceTypeNameForType(Type type)
        {
            // TODO: use conventions
            var attrs = type.CustomAttributes.Where(x => x.AttributeType == typeof(Newtonsoft.Json.JsonObjectAttribute)).ToList();

            string title = type.Name;
            if (attrs.Any())
            {
                var titles = attrs.First().NamedArguments.Where(arg => arg.MemberName == "Title")
                    .Select(arg => arg.TypedValue.Value.ToString()).ToList();
                if (titles.Any()) title = titles.First();
            }

            return FormatPropertyName(_pluralizationService.Pluralize(title)).Dasherize();
        }

        /// <summary>
        /// Determines the key that a property will be serialized as.
        /// </summary>
        /// <param name="propInfo">The property</param>
        /// <returns>The key to serialize the given property as</returns>
        protected internal virtual string CalculateJsonKeyForProperty(PropertyInfo propInfo)
        {
            // TODO: use conventions
            var jsonPropertyAttribute = (JsonPropertyAttribute)propInfo.GetCustomAttributes(typeof (JsonPropertyAttribute)).FirstOrDefault();
            return jsonPropertyAttribute != null ? jsonPropertyAttribute.PropertyName : FormatPropertyName(propInfo.Name);
        }

        private static string FormatPropertyName(string propertyName)
        {
            string result = propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
            return result;
        }

        /// <summary>
        /// Calculates the ID property for a given resource type.
        /// </summary>
        /// <param name="type">The type to use to calculate the ID for</param>
        /// <returns>The ID property to use for this type</returns>
        protected virtual PropertyInfo CalculateIdProperty(Type type)
        {
            return
                type
                    .GetProperties()
                    .FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(UseAsIdAttribute)))
                ?? type.GetProperty("Id");
        }
    }
}
