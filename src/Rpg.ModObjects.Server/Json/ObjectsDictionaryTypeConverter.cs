using Rpg.ModObjects.Reflection;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class ObjectsDictionaryTypeConverter : JsonConverter<ObjectsDictionary>
    {
        private readonly IEnumerable<Type> _types;

        public ObjectsDictionaryTypeConverter()
        {
            _types = RpgTypeScan.ForSubTypes(typeof(RpgObject)).Where(x => !x.IsGenericType && !x.IsAbstract);
        }

        public override ObjectsDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var dictionary = new ObjectsDictionary();

                // Iterate over each property in the JSON document
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Value.TryGetProperty("$type", out var typeProp))
                    {
                        var valType = _types.FirstOrDefault(x => x.Name == typeProp.GetString());
                        if (valType != null)
                        {
                            var val = (RpgObject?)JsonSerializer.Deserialize(property.Value, valType, options);
                            if (val != null)
                                dictionary.Add(property.Name, val);
                        }
                    }
                }

                return dictionary;
            }
        }

        public override void Write(Utf8JsonWriter writer, ObjectsDictionary value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                if (kvp.Value != null)
                { 
                    writer.WritePropertyName(kvp.Key.ToString());
                    JsonSerializer.Serialize(writer, kvp.Value, typeof(RpgObject), options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
