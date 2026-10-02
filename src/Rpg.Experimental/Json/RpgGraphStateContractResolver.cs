using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rpg.Experimental.Reflection;
using System.Collections;
using System.Reflection;

namespace Rpg.Experimental.Json
{
    public class RpgGraphStateContractResolver : CamelCasePropertyNamesContractResolver
    {
        private bool _serializeCollectionProperties = false;
        private bool _omitDefaults = false;

        /// <param name="serializeCollectionProperties">Write child objects and collections of child objects</param>
        /// <param name="omitDefaults">
        /// Leave out any property whose value is what a newly created object of that type already has. This
        /// makes saved text much smaller. It is safe because restoring creates the object the same way, so
        /// the property gets the same value without being told.
        /// </param>
        public RpgGraphStateContractResolver(bool serializeCollectionProperties, bool omitDefaults = false)
        {
            _serializeCollectionProperties = serializeCollectionProperties;
            _omitDefaults = omitDefaults;
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = false,
                OverrideSpecifiedNames = true
            };
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // check member types
            if (!_serializeCollectionProperties && member.MemberType == MemberTypes.Property && property.PropertyType != null)
            {
                if (RpgTypeUtilities.PropertyIsEnumerableOfType(property.PropertyType, typeof(RpgObject)))
                    property.ShouldSerialize = _ => false;
                else if (RpgTypeUtilities.PropertyOfType(property.PropertyType, typeof(RpgObject)))
                    property.ShouldSerialize = _ => false;
            }

            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            if (!_omitDefaults)
                return properties;

            //The object a restore would start from, created the way a restore creates it
            var newlyCreated = new Lazy<object?>(() => CreateAsRestoreWould(type));

            foreach (var property in properties)
            {
                var valueProvider = property.ValueProvider;
                if (valueProvider == null || !property.Readable)
                    continue;

                var shouldSerialize = property.ShouldSerialize;
                property.ShouldSerialize = instance =>
                {
                    if (shouldSerialize != null && !shouldSerialize(instance))
                        return false;

                    var template = newlyCreated.Value;
                    if (template == null)
                        return true;

                    try
                    {
                        return !SameValue(valueProvider.GetValue(instance), valueProvider.GetValue(template));
                    }
                    catch
                    {
                        return true;
                    }
                };
            }

            return properties;
        }

        private object? CreateAsRestoreWould(Type type)
        {
            try
            {
                if (type.IsAbstract || type.IsInterface)
                    return null;

                if (ResolveContract(type) is not JsonObjectContract contract)
                    return null;

                //A constructor marked for json is used ahead of any other
                if (contract.OverrideCreator != null)
                    return contract.CreatorParameters.Count == 0
                        ? contract.OverrideCreator()
                        : null;

                if (contract.DefaultCreator != null && !contract.DefaultCreatorNonPublic)
                    return contract.DefaultCreator();

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool SameValue(object? value, object? newlyCreatedValue)
        {
            if (value == null)
                return newlyCreatedValue == null;

            if (newlyCreatedValue == null)
                return false;

            if (value is string || value.GetType().IsValueType)
                return value.Equals(newlyCreatedValue);

            //An empty collection is restored as the empty collection the object is created with
            if (value is ICollection collection && newlyCreatedValue is ICollection newlyCreatedCollection)
                return collection.Count == 0 && newlyCreatedCollection.Count == 0;

            return false;
        }
    }
}
