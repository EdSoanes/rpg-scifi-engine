using Rpg.ModObjects.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class PolymorphicTypeConverter<T> : JsonConverter<T>
    {
        private readonly IEnumerable<Type> _types;

        public PolymorphicTypeConverter()
        {
            var type = typeof(T);
            _types = RpgTypeScan.ForSubTypes(type).Where(x => !x.IsGenericType && !x.IsAbstract);
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return _types.Contains(typeToConvert) || typeof(T) == typeToConvert;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                if (!_types.Any())
                    return JsonSerializer.Deserialize<T>(jsonDocument, options)!;

                if (!jsonDocument.RootElement.TryGetProperty("$type", out var typeProperty))
                    return JsonSerializer.Deserialize<T>(jsonDocument, options)!;

                var type = _types.FirstOrDefault(x => x.Name == typeProperty.GetString());
                if (type == null)
                    throw new JsonException();

                var jsonObject = jsonDocument.RootElement.GetRawText();
                return (T)JsonSerializer.Deserialize(jsonDocument, type, options)!;
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", value!.GetType().Name);

            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            var document = JsonDocument.Parse(json);

            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new NotSupportedException();
            }

            foreach (var property in root.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
