using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Server.Json
{
    public class RpgJson
    {
        private static JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new PointInTimeConverter(),
                new PropRefConverter(),
                new DiceConverter()
            }
        };

        public static string Serialize(object obj)
            => JsonSerializer.Serialize(obj, serializeOptions);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonSerializer.Deserialize<T>(json, serializeOptions)!;
    }
}
