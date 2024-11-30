using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Rpg.ModObjects.Server.Json
{
    public class RpgJson
    {
        public static JsonSerializerSettings? _serializerOptions;

        public static JsonSerializerSettings SerializerOptions()
        {
            if (_serializerOptions == null)
            {
                _serializerOptions = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = false,
                            OverrideSpecifiedNames = true
                        }
                    }
                };

                _serializerOptions.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            }

            return _serializerOptions;
        }

        public static string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, SerializerOptions());

        public static T Deserialize<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, SerializerOptions())!;

        public static T Deserialize<T>(Type type, string json)
            where T : class
                => (T)JsonConvert.DeserializeObject(json, type, SerializerOptions())!;
    }
}
