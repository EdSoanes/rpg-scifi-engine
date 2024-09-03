using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
