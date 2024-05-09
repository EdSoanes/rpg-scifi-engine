using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Rpg.Sys
{
    public static class GraphExtensions
    {
        private static List<string> ScannableNamespaces = new List<string>();
        private static Type[] PermittedModPropReturnTypes = new Type[]
        {
            typeof(int),
            typeof(Dice)
        };

        public static void RegisterAssembly(Assembly assembly)
        {
            var namespaces = assembly.GetTypes()
                .Select(t => t.Namespace)
                .Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("System.") && !x.StartsWith("Microsoft."))
                .Distinct()
                .Cast<string>();

            ScannableNamespaces.AddRange(namespaces);
            ScannableNamespaces = ScannableNamespaces.Distinct().ToList();
        }

        public static string[] GetBaseTypes(this ModObject entity)
        {
            var res = new List<string>();
            var t = entity.GetType();
            while (t != null)
            {
                res.Add(t.Name);
                t = t == typeof(ModObject)
                    ? null
                    :t.BaseType;
            }

            res.Reverse();
            return res.ToArray();
        }

        public static ModObject? FindModdableObject(this object obj, Guid id)
            => obj.Traverse().FirstOrDefault(x => x.Id == id);

        public static void ForEach(this object obj, Mod[] mods, Action<ModObject, string, Mod[]> onMatch)
        {
            var modsByEntity = mods.GroupBy(x => x.EntityId);
            var entityIds = modsByEntity.Select(x => x.Key);
            foreach (var entity in obj.Traverse().Where(x => entityIds.Contains(x.Id)))
            {
                var modsByProp = modsByEntity
                    .First(x => x.Key == entity.Id)
                    .GroupBy(x => x.Prop);

                foreach (var propMods in modsByProp)
                    onMatch(entity, propMods.Key, propMods.ToArray());
            }
        }

        public static void ForEach(this object obj, Action<ModObject, string> onMatch)
        {
            foreach (var entity in obj.Traverse())
                foreach (var prop in entity.ModdableProperties())
                    onMatch(entity, prop.Name);
        }

        public static void ForEachReversed(this object obj, Action<ModObject, string> onMatch)
        {
            foreach (var entity in obj.Traverse(true))
                foreach (var prop in entity.ModdableProperties())
                    onMatch(entity, prop.Name);
        }


        public static IEnumerable<ModObject> Traverse(this object obj, bool bottomUp = false)
        {
            var entity = obj as ModObject;
            if (entity != null && !bottomUp)
                yield return entity;

            if (!obj.GetType().IsPrimitive)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                    foreach (var item in items)
                    {
                        var childEntities = item.Traverse(bottomUp);
                        foreach (var childEntity in childEntities)
                            yield return childEntity;
                    }
                }
            }

            if (entity != null && bottomUp)
                yield return entity;
        }

        public static List<ModObject> Descendants(this object obj)
        {
            var res = new List<ModObject>();
            var entity = obj as ModObject;
            if (entity != null)
                res.Add(entity);

            if (!obj.GetType().IsPrimitive)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                    foreach (var item in items)
                    {
                        var childEntities = Descendants(item);
                        res.AddRange(childEntities);
                    }
                }
            }

            return res;
        }

        public static object? PropertyValue(this object? entity, string path)
        {
            if (entity == null || string.IsNullOrEmpty(path))
                return null;

            object? res = entity;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                var propInfo = res.GetType().GetProperty(part);
                res = propInfo?.GetValue(res, null);
                if (res == null)
                    break;
            }

            return res;
        }

        public static T? PropertyValue<T>(this ModObject? entity, string path)
        {
            if (entity == null)
                return default;

            if (string.IsNullOrEmpty(path))
                return entity is T
                    ? (T?)(object)entity
                    : default;

            object? res = entity;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                var propInfo = res.GetType().GetProperty(part);
                res = propInfo?.GetValue(res, null);
                if (res == null)
                    break;
            }

            if (res is T)
                return (T?)res;

            if (typeof(T) == typeof(string))
                return (T?)(object?)res?.ToString();

            return default;
        }

        public static void PropertyValue(this object? entity, string path, object? value)
        {
            if (value == null || !PermittedModPropReturnTypes.Any(x => x == value.GetType()))
                return;

            if (entity == null || string.IsNullOrEmpty(path))
                return;


            var parts = path.Split('.');
            if (parts.Length > 0)
            {
                object? res = entity;

                var prop = parts.Last();
                if (parts.Length > 1)
                {
                    foreach (var part in parts.Take(parts.Length - 1))
                    {
                        var propInfo = res.ModdableProperty(part);
                        res = propInfo?.GetValue(res, null);
                        if (res == null)
                            break;
                    }
                }

                var propertyInfo = res?.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo?.SetMethod != null)
                {
                    if (propertyInfo.PropertyType == typeof(int) && value is Dice)
                        propertyInfo?.SetValue(res, ((Dice)value).Roll());
                    else
                        propertyInfo?.SetValue(res, (Dice)value);
                } 
            }
        }

        public static TR? ExecuteFunction<TR>(this object? obj, string method) => obj._ExecuteFunction<TR>(method);
        public static TR? ExecuteFunction<T1, TR>(this object? obj, string method, T1? arg1) => obj._ExecuteFunction<TR>(method, new object?[] { arg1 });
        public static TR? ExecuteFunction<T1, T2, TR>(this object? obj, string method, T1? arg1, T2? arg2) => obj._ExecuteFunction<TR>(method, new object?[] { arg1, arg2 });
        public static TR? ExecuteFunction<T1, T2, T3, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3) => obj._ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3 });
        public static TR? ExecuteFunction<T1, T2, T3, T4, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4) => obj._ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3, arg4 });
        public static TR? ExecuteFunction<T1, T2, T3, T4, T5, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5) => obj._ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3, arg4, arg5 });

        private static TR? _ExecuteFunction<TR>(this object? obj, string method, object?[]? args = null)
        {
            var methodInfo = obj.GetMethodInfo(method);

            var res = methodInfo.IsStatic
                ? (TR?)methodInfo?.Invoke(null, args)
                : (TR?)methodInfo?.Invoke(obj, args);

            return res;
        }

        public static void ExecuteAction(this object? obj, string method) => obj._ExecuteAction(method);
        public static void ExecuteAction<T1>(this object? obj, string method, T1? arg1) => obj._ExecuteAction(method, new object?[] { arg1 });
        public static void ExecuteAction<T1, T2>(this object? obj, string method, T1? arg1, T2? arg2) => obj._ExecuteAction(method, new object?[] { arg1, arg2 });
        public static void ExecuteAction<T1, T2, T3>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3) => obj._ExecuteAction(method, new object?[] { arg1, arg2, arg3 });
        public static void ExecuteAction<T1, T2, T3, T4>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4) => obj._ExecuteAction(method, new object?[] { arg1, arg2, arg3, arg4 });
        public static void ExecuteAction<T1, T2, T3, T4, T5>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5) => obj._ExecuteAction(method, new object?[] { arg1, arg2, arg3, arg4, arg5 });

        private static void _ExecuteAction(this object? obj, string method, object?[]? args = null)
        {
            var methodInfo = obj.GetMethodInfo(method);

            if (methodInfo.IsStatic)
                methodInfo.Invoke(null, args);
            else
                methodInfo.Invoke(obj, args);
        }

        private static MethodInfo GetMethodInfo(this object? obj, string method)
        {
            var parts = method.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 1 || parts.Length > 2)
                throw new ArgumentException($"Invalid segment count in {method}$");

            var className = parts.Length == 2 ? parts[0] : null;
            var methodName = parts.Length == 2 ? parts[1] : parts[0];

            Type? type = null;
            if (!string.IsNullOrEmpty(className))
            {
                type = Assembly.GetAssembly(typeof(Dice))!
                    .GetTypes()
                    .FirstOrDefault(x => x.Name == className);

                if (type == null)
                    type = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(x => x.Name == className);

                if (type == null)
                    type = Assembly.GetCallingAssembly()
                        .GetTypes()
                        .FirstOrDefault(x => x.Name == className);
            }

            type ??= obj?.GetType();
            if (type == null)
                throw new ArgumentException($"Could not determine type for {method}$");

            var methodInfo = type?.GetMethod(methodName);
            if (methodInfo == null)
                throw new ArgumentException($"Method in {method} does not exist");

            return methodInfo;
        }



        internal static PropertyInfo? ModdableProperty(this object context, string prop)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == prop && x.IsModdableProperty());
        }

        internal static PropertyInfo[] ModdableProperties(this object context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsModdableProperty)
                .ToArray();
        }

        private static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            if (!InScannableNamespace(propertyInfo.DeclaringType))
                return false;

            if (!PermittedModPropReturnTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily);
        }

        private static bool InScannableNamespace(Type? type)
        {
            return type == null
                ? false
                : ScannableNamespaces.Any(x => type.Namespace == x);
        }

        internal static IEnumerable<object> PropertyObjects(this object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            try
            {
                isEnumerable = false;

                if (PermittedModPropReturnTypes.Any(x => x == propertyInfo.PropertyType))
                    return Enumerable.Empty<object>();

                var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                if (obj == null || obj is string || obj.GetType().IsPrimitive || obj is Guid)
                    return Enumerable.Empty<object>();

                if (obj is IEnumerable)
                {
                    var res = new List<object>();
                    if (obj is ModObject)
                        res.Add(obj);

                    isEnumerable = true;
                    res.AddRange((obj as IEnumerable)!.Cast<object>());

                    return res;
                }

                return new List<object> { obj };
            }
            catch (Exception ex)
            {
                //What happened?
                var x = ex;
                isEnumerable = false;
                return Enumerable.Empty<object>();
            }
        }
    }
}
