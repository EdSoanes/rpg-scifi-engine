using Rpg.ModObjects.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Json
{
    public class DiceConverter : JsonConverter<Dice>
    {
        public override Dice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            return new Dice(str);
        }

        public override void Write(Utf8JsonWriter writer, Dice value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.IsConstant ? value.Roll().ToString() : value.ToString());
        }
    }
}
