using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public static class RpgReflection
    {
        internal static string[] GetArchetypes(this Type type)
        {
            var res = new List<string>();
            if (type.IsAssignableTo(typeof(RpgObject)))
            {
                while (type != null)
                {
                    res.Add(type.Name);
                    if (type == typeof(RpgObject) || type.BaseType == null)
                        break;

                    type = type.BaseType;
                }
            }

            res.Reverse();
            return res.ToArray();
        }

        internal static PropertyInfo[] ScanForModdableProperties(this RpgObject context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsModdableProperty)
                .ToArray();
        }

        internal static PropertyInfo[] ScanForChildProperties(this RpgObject context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsChildProperty)
                .ToArray();
        }

        internal static (int?, int?) GetPropertyThresholds(this PropertyInfo propInfo)
        {
            if (propInfo.PropertyType == typeof(int))
            {
                var threshold = propInfo.GetCustomAttribute<ThresholdAttribute>();
                if (threshold != null)
                    return (threshold.Min, threshold.Max);

                var select = propInfo.GetCustomAttribute<MetaSelectAttribute>();
                if (select != null)
                    return (select.Min, select.Max);

                var integer = propInfo.GetCustomAttribute<IntegerAttribute>();
                if (integer != null && (integer.Min > int.MinValue || integer.Max < int.MaxValue))
                    return (integer.Min, integer.Max);
            }

            return (null, null);
        }

        internal static PropertyInfo? ScanForProperty(this object? entity, string path, out object? pathEntity)
        {
            pathEntity = null;
            if (entity == null || string.IsNullOrEmpty(path))
                return null;

            var parts = path.Split('.');
            PropertyInfo? propInfo = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                propInfo = entity.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo == null)
                    return null;

                if (i < parts.Length - 1)
                {
                    entity = propInfo.GetValue(entity, null);
                    if (entity == null)
                        return null;
                }
            }

            if (propInfo != null)
            {
                pathEntity = entity;
                return propInfo;
            }

            pathEntity = null;
            return null;
        }

        internal static PropertyInfo? ScanForModdableProperty(this object? entity, string path, out object? pathEntity)
        {
            var propInfo = ScanForProperty(entity, path, out pathEntity);
            if (!propInfo.IsModdableProperty())
            {
                pathEntity = null;
                return null;
            }

            return propInfo;
        }

        private static bool IsModdableProperty(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null || !RpgTypeScan.RpgPropertyTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            if (propertyInfo.PropertyType == typeof(string))
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily);
        }

        private static bool IsChildProperty(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return false;

            return propertyInfo.PropertyType.IsAssignableTo(typeof(RpgObject))
                || Nullable.GetUnderlyingType(propertyInfo.PropertyType) == typeof(RpgObject);

        }

        internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types
                    .Where(t => t != null)
                    .Cast<Type>();
            }
        }
    }
}
