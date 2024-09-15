using Rpg.ModObjects.Props;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class PropsDictionaryTypeConverter : JsonConverter<PropsDictionary>
    {
        public PropsDictionaryTypeConverter()
        {
        }

        public override PropsDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var dictionary = new PropsDictionary();

                // Iterate over each property in the JSON document
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    var val = (Prop?)JsonSerializer.Deserialize(property.Value, typeof(Prop), options);
                    if (val != null)
                        dictionary.Add(property.Name, val);
                }

                return dictionary;
            }
        }

        public override void Write(Utf8JsonWriter writer, PropsDictionary value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                if (kvp.Value != null)
                { 
                    writer.WritePropertyName(kvp.Key.ToString());
                    JsonSerializer.Serialize(writer, kvp.Value, typeof(Prop), options);
                }
            }

            writer.WriteEndObject();
        }

        public override PropsDictionary ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Read(ref reader, typeToConvert, options);

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] PropsDictionary value, JsonSerializerOptions options)
            => Write(writer, value, options);
    }
}
