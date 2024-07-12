using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rpg.ModObjects
{
    public class RpgSerializer
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, JsonSettings);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonConvert.DeserializeObject<T>(json, JsonSettings)!;

        public static T? Deserialize<T>(Type type, string json)
            where T : RpgObject
                => JsonConvert.DeserializeObject(json, type, JsonSettings)! as T;
    }
}
