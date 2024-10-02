using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection.Args;
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
            TypeInfoResolver = new PolymorphicTypeResolver()
                .Register<RpgObject>()
                .Register<Mod>()
                .Register<ModSet>()
                .Register<BaseBehavior>()
                .Register<State>()
                .Register<RpgArg>(),
            Converters =
            {
                new RpgObjectCollectionTypeConverter(),
                new PointInTimeConverter(),
                new PropRefConverter(),
                new DiceConverter(),
                new ObjectsDictionaryTypeConverter(),
                new ModSetsDictionaryTypeConverter(),
                new StatesDictionaryTypeConverter(),
                new ActionsDictionaryTypeConverter(),
            }
        };

        public static string Serialize(object obj)
            => JsonSerializer.Serialize(obj, serializeOptions);

        public static T Deserialize<T>(string json)
            where T : class
                => JsonSerializer.Deserialize<T>(json, serializeOptions)!;

        public static T Deserialize<T>(Type type, string json)
            where T : class
                => (T)JsonSerializer.Deserialize(json, type, serializeOptions)!;
    }
}
