using Rpg.ModObjects.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Rpg.ModObjects.Server.Json
{
    public interface IDerivedTypeFactory
    {
        public bool CanHandle(JsonTypeInfo typeInfo);
        public List<JsonDerivedType> GetDerivedTypes(JsonTypeInfo typeInfo);
    }

    public class DerivedTypeFactory<T> : IDerivedTypeFactory
    {
        private List<JsonDerivedType>? _derivedTypes = null;

        public bool CanHandle(JsonTypeInfo typeInfo)
            => typeInfo.Type.IsAssignableTo(typeof(T));

        public List<JsonDerivedType> GetDerivedTypes(JsonTypeInfo typeInfo)
        {
            if (_derivedTypes == null)
            {
                var subTypes = RpgTypeScan.ForTypes<T>().ToArray();
                _derivedTypes = subTypes
                    .Select(x => new JsonDerivedType(x, x.Name))
                    .ToList();
            }
                
            return _derivedTypes;
        }
    }

    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        private static List<IDerivedTypeFactory> _factories = new();

        public static void Register<T>()
            => _factories.Add(new DerivedTypeFactory<T>());

        public static void Register(IDerivedTypeFactory derivedTypeFactory)
            => _factories.Add(derivedTypeFactory);

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
            foreach (var factory in _factories)
            {
                if (factory.CanHandle(jsonTypeInfo))
                {
                    jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                    {
                        TypeDiscriminatorPropertyName = "$type",
                        IgnoreUnrecognizedTypeDiscriminators = true,
                        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,

                    };

                    foreach (var item in factory.GetDerivedTypes(jsonTypeInfo))
                        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(item);
                }
            }

            return jsonTypeInfo;
        }
    }
}
