using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Tests
{
    public class JsonTestClass
    {
        public PointInTime Time { get; init; }
        public SpanOfTime Lifespan { get; init; }
        public PropRef PropRef { get; init; }
        public Dice DiceNumber { get; init; }
        public Dice Dice {  get; init; }

        [JsonInclude] public string PrivateSet { get; private set; }
        [JsonInclude] private string PrivateGetSet { get; set; }
        [JsonConstructor] private JsonTestClass() { }

        public JsonTestClass(string ps)
        {
            PrivateSet = ps;
            PrivateGetSet = ps;
        }
    }

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

    public class JsonFormattingTests
    {
        JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new PointInTimeConverter()
            }
        };

        
    }
}
