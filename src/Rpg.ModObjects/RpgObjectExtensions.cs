using NanoidDotNet;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        private const int Size = 12;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";

        public static T AddModSet<T>(this T entity, string name, System.Action<ModSet> addAction)
            where T : RpgObject
        {
            var modSet = new ModSet(entity.Id, name);

            addAction.Invoke(modSet);
            entity.AddModSet(modSet);

            return entity;
        }

        internal static object? PropertyValue(this object? entity, string path, out bool propExists)
        {
            var propInfo = RpgReflection.ScanForProperty(entity, path, out var pathEntity);

            propExists = pathEntity != null;
            var val = propInfo?.GetValue(pathEntity, null);

            return val;
        }

        internal static T? PropertyValue<T>(this RpgObject? entity, string path)
        {
            var propInfo = RpgReflection.ScanForProperty(entity, path, out var pathEntity);
            var res = propInfo?.GetValue(pathEntity, null);

            if (res is T)
                return (T?)res;

            if (typeof(T) == typeof(string))
                return (T?)(object?)res?.ToString();

            return default;
        }

        internal static void PropertyValue(this object? entity, string path, object? value)
        {
            var propInfo = RpgReflection.ScanForModdableProperty(entity, path, out var pathEntity);
            if (propInfo?.SetMethod != null)
            {
                if (propInfo.PropertyType == typeof(int) && value is Dice)
                    propInfo?.SetValue(pathEntity, ((Dice)value).Roll());
                else
                    propInfo?.SetValue(pathEntity, (Dice)value);
            }
        }
    }
}
