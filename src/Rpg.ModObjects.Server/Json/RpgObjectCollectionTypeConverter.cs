using Rpg.ModObjects.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class RpgObjectCollectionTypeConverter : JsonConverter<RpgObjectCollection>
    {
        public RpgObjectCollectionTypeConverter()
        {
        }

        public override RpgObjectCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var collection = new RpgObjectCollection();

                var entityId = doc.RootElement.GetProperty("entityId").GetString();
                if (!string.IsNullOrEmpty(entityId))
                    collection.EntityId = entityId;

                var containerName = doc.RootElement.GetProperty("containerName").GetString();
                if (!string.IsNullOrEmpty(containerName))
                    collection.ContainerName = containerName;

                foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
                {
                    var archetype = item.GetProperty("archetype").GetString();
                    var type = RpgTypeScan.ForTypeByName(archetype!);
                    var val = (RpgObject?)JsonSerializer.Deserialize(item, type!, options);
                    if (val != null)
                        collection.Add(val);
                }

                return collection;
            }
        }

        public override void Write(Utf8JsonWriter writer, RpgObjectCollection value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("entityId", value.EntityId);
            writer.WriteString("containerName", value.ContainerName);
            writer.WriteNumber("maxItems", value.MaxItems);
            writer.WriteStartArray("items");
            foreach (var obj in value)
            {
                if (obj != null)
                    JsonSerializer.Serialize(writer, obj, obj.GetType(), options);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
