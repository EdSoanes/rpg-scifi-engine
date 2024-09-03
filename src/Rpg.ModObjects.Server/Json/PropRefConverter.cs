using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
