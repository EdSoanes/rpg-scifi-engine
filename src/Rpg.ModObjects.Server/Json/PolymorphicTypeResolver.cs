using Rpg.ModObjects.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Rpg.ModObjects.Server.Json
{
    public interface IDerivedTypeFactory
    {
        public bool CanHandle(JsonTypeInfo typeInfo);
        public List<JsonDerivedType> GetDerivedTypes(Type type);
    }

    public class DerivedTypeFactory<T> : IDerivedTypeFactory
    {
        private Dictionary<string, List<JsonDerivedType>> _derivedTypes = new();

        public bool CanHandle(JsonTypeInfo typeInfo)
            => typeInfo.Type.IsAssignableTo(typeof(T)) && typeInfo.Kind == JsonTypeInfoKind.Object;

        public List<JsonDerivedType> GetDerivedTypes(Type type)
        {
            if (!_derivedTypes.ContainsKey(type.Name))
            {
                var subTypes = RpgTypeScan.ForSubTypes(type)
                    .Where(x => !x.IsGenericType && !x.IsAbstract)
                    .Select(x => new JsonDerivedType(x, x.Name))
                    .ToList();

                _derivedTypes.Add(type.Name, subTypes);
            }
                
            return _derivedTypes[type.Name];
        }
    }

    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        private List<IDerivedTypeFactory> _factories = new();

        public PolymorphicTypeResolver Register<T>()
        {
            _factories.Add(new DerivedTypeFactory<T>());
            return this;
        }

        //public PolymorphicTypeResolver() : base() 
        //{
        //    Modifiers.Add(jsonTypeInfo =>
        //    {
        //        var propInfo = jsonTypeInfo.CreateJsonPropertyInfo(typeof(string), "$type");
        //        propInfo.Get = (obj) => jsonTypeInfo.Type.Name;
        //        propInfo.Set = (obj, val) => { };

        //        jsonTypeInfo.Properties.Add(propInfo);
        //    });
        //}

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
            foreach (var factory in _factories)
            {
                if (factory.CanHandle(jsonTypeInfo))
                {
                    var derivedTypes = factory.GetDerivedTypes(type);
                    if (derivedTypes.Any())
                    {
                        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                        {
                            TypeDiscriminatorPropertyName = "$type",
                            IgnoreUnrecognizedTypeDiscriminators = true,
                            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                        };

                        foreach (var item in derivedTypes)
                            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(item);
                    }

                    break;
                }
            }

            return jsonTypeInfo;
        }
    }
}
