using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Rpg.Experimental.Server
{
    /// <summary>
    /// The json a host sends to and receives from a user interface. It is plain data: no .net type names,
    /// camel case names, enums as text and dice expressions as text. The character sheet itself travels
    /// inside it as the compressed text given by RpgCharacterSheet.Save().
    /// </summary>
    public static class RpgServerJson
    {
        private static JsonSerializerSettings? _settings;

        public static JsonSerializerSettings Settings()
        {
            if (_settings == null)
            {
                var settings = new JsonSerializerSettings();
                Configure(settings);
                _settings = settings;
            }

            return _settings;
        }

        /// <summary>
        /// Apply the settings to serializer settings owned by a host, e.g. those of a web framework
        /// </summary>
        public static void Configure(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }
            };

            if (!settings.Converters.OfType<StringEnumConverter>().Any())
                settings.Converters.Add(new StringEnumConverter());

            if (!settings.Converters.OfType<DiceJsonConverter>().Any())
                settings.Converters.Add(new DiceJsonConverter());
        }

        public static string Serialize(object? obj)
            => JsonConvert.SerializeObject(obj, Settings());

        public static T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json, Settings())!;
    }

    /// <summary>
    /// A dice expression as text, e.g. "2d6 + 1" or "7"
    /// </summary>
    public class DiceJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Dice) || objectType == typeof(Dice?);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(((Dice)value).ToString());
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return objectType == typeof(Dice?) ? null : Dice.Zero;

            //Also accept the form the engine itself saves: { "expr": "2d6" }
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JObject.Load(reader);
                var expr = obj.GetValue("expr", StringComparison.OrdinalIgnoreCase)?.ToString();
                return new Dice(expr);
            }

            return new Dice(reader.Value?.ToString());
        }
    }
}
