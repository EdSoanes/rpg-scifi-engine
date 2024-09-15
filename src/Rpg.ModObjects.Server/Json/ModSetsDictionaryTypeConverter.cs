using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class ModSetsDictionaryTypeConverter : JsonConverter<ModSetDictionary>
    {
        private readonly IEnumerable<Type> _types;

        public ModSetsDictionaryTypeConverter()
        {
            _types = RpgTypeScan.ForSubTypes(typeof(ModSet)).Where(x => !x.IsGenericType && !x.IsAbstract);
        }

        public override ModSetDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var dictionary = new ModSetDictionary();

                // Iterate over each property in the JSON document
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Value.TryGetProperty("$type", out var typeProp))
                    {
                        var valType = _types.FirstOrDefault(x => x.Name == typeProp.GetString());
                        if (valType != null)
                        {
                            var val = (ModSet?)JsonSerializer.Deserialize(property.Value, valType, options);
                            if (val != null)
                                dictionary.Add(property.Name, val);
                        }
                    }
                }

                return dictionary;
            }
        }

        public override void Write(Utf8JsonWriter writer, ModSetDictionary value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                if (kvp.Value != null)
                { 
                    writer.WritePropertyName(kvp.Key.ToString());
                    JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
