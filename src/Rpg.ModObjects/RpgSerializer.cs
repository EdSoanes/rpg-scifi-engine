using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Rpg.ModObjects
{
    public class RpgSerializer
    {
        private static JsonSerializerSettings? _jsonSerializerSettings = null;
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_jsonSerializerSettings == null)
                {
                    _jsonSerializerSettings = new JsonSerializerSettings
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

                    _jsonSerializerSettings.Converters.Add(new StringEnumConverter());
                }

                return _jsonSerializerSettings;
            }
        } 

        public static string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, JsonSerializerSettings);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings)!;

        public static T? Deserialize<T>(Type type, string json)
            where T : RpgObject
                => JsonConvert.DeserializeObject(json, type, JsonSerializerSettings)! as T;
    }
}
