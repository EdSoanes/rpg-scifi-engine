using Rpg.ModObjects.Time;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class PointInTimeConverter : JsonConverter<PointInTime>
    {
        public override PointInTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return PointInTime.FromString(str);
        }

        public override void Write(Utf8JsonWriter writer, PointInTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
