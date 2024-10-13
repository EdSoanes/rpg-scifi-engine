
using Rpg.ModObjects.Props;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class PropRefConverter : JsonConverter<PropRef>
    {
        public override PropRef Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return PropRef.FromString(str!);
        }

        public override void Write(Utf8JsonWriter writer, PropRef value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
