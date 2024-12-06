using NanoidDotNet;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        private const int Size = 12;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";

        public static void SetProperty<T>(this RpgObject? obj, string prop, T? value)
        {
            var propInfo = RpgReflection.ScanForProperty(obj, prop, out var target);
            if (target != null && propInfo != null
                && (propInfo.PropertyType.IsAssignableFrom(typeof(T)) || (Nullable.GetUnderlyingType(propInfo.PropertyType)?.IsAssignableFrom(typeof(T)) ?? false)))
            {
                var x = propInfo.GetSetMethod(true);
                x?.Invoke(target, [value]);
            }
        }

        public static T? GetProperty<T>(this RpgObject? obj, string prop)
        {
            var propInfo = RpgReflection.ScanForProperty(obj, prop, out var target);
            if (propInfo != null
                && (propInfo.PropertyType.IsAssignableFrom(typeof(T)) || (Nullable.GetUnderlyingType(propInfo.PropertyType)?.IsAssignableFrom(typeof(T)) ?? false)))
            {
                var val = propInfo.GetValue(target);
                return val != null ? (T)val : default;
            }

            return default;
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
    }
}
