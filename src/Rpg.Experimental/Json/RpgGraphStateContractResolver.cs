using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rpg.Experimental.Reflection;
using System.Reflection;

namespace Rpg.Experimental.Json
{
    public class RpgGraphStateContractResolver : CamelCasePropertyNamesContractResolver
    {
        private bool _serializeCollectionProperties = false;

        public RpgGraphStateContractResolver(bool serializeCollectionProperties) 
        {
            _serializeCollectionProperties = serializeCollectionProperties;
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
    }
}
