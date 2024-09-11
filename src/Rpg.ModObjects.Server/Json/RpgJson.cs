using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Rpg.ModObjects.Server.Json
{
    public class RpgJson
    {
        private static JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new PolymorphicTypeResolver(),
            Converters =
            {
                new PointInTimeConverter(),
                new PropRefConverter(),
                new DiceConverter(),
                new PolymorphicTypeConverter<RpgContainer>(),
                new PolymorphicTypeConverter<RpgComponent>(),
                new PolymorphicTypeConverter<RpgEntity>(),
                new PolymorphicTypeConverter<RpgObject>(),
                new PolymorphicTypeConverter<Mod>(),
                new PolymorphicTypeConverter<ModSet>(),
                new PolymorphicTypeConverter<BaseBehavior>(),
                new PolymorphicTypeConverter<State>()
            }
        };

        public static string Serialize(object obj)
            => JsonSerializer.Serialize(obj, serializeOptions);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonSerializer.Deserialize<T>(json, serializeOptions)!;
    }
}
