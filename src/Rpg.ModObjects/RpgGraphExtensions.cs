using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;
using System.Collections;
using System.Reflection;
using System.Security.AccessControl;

namespace Rpg.ModObjects
{
    public static class RpgGraphExtensions
    {
        public static List<string> ScannableNamespaces = new List<string>();
        private static Type[] ModdablePropertyTypes = new Type[]
        {
            typeof(int),
            typeof(Dice)
        };

        private static Type[] ExcludedPropertyTypes = new Type[]
        {
            typeof(ModSetStore),
            typeof(PropStore),
            typeof(RpgGraph),
            typeof(Modification),
            typeof(ModSet),
            typeof(Mod),
            typeof(string),
            typeof(DateTime),
            typeof(Guid)
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

        internal static string[] GetBaseTypes(this RpgObject entity)
        {
            var res = new List<string>();
            var t = entity.GetType();
            while (t != null)
            {
                res.Add(t.Name);
                t = t == typeof(RpgObject)
                    ? null
                    :t.BaseType;
            }

            res.Reverse();
            return res.ToArray();
        }

        internal static IEnumerable<RpgObject> Traverse(this object obj, bool bottomUp = false)
        {
            var entity = obj as RpgObject;
            if (entity != null && !bottomUp)
                yield return entity;

            if (!IsExcludedType(obj.GetType()))
            {
                var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var items = obj.GetPropertyObjects(propertyInfo, out var isEnumerable);
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

        internal static string[] PathTo(this object obj, object? descendent)
        {
            var propStack = new Stack<string>();

            if (descendent != null)
            {
                var res = PathTo(propStack, obj, descendent);
                if (!res)
                    return new string[0];
            }

            return propStack.ToArray();
        }

        private static bool PathTo(Stack<string> propStack, object obj, object descendent)
        {
            if (obj == descendent)
                return true;

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                propStack.Push(propertyInfo.Name);

                var children = obj.GetPropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                if (isEnumerable)
                    return false;

                if (children.Count() == 1 && PathTo(propStack, children.First(), descendent))
                    return true;

                propStack.Pop();
            }

            return false;
        }

        internal static object? PropertyValue(this object? entity, string path, out bool propExists)
        {
            propExists = false;
            if (entity == null || string.IsNullOrEmpty(path))
                return null;

            object? res = entity;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                var propInfo = res.GetType().GetProperty(part);
                if (propInfo == null)
                    return null;

                res = propInfo.GetValue(res, null);
                if (res == null)
                {
                    propExists = true;
                    return null;
                }
            }

            propExists = true;
            return res;
        }

        internal static T? PropertyValue<T>(this RpgObject? entity, string path)
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

        internal static void PropertyValue(this object? entity, string path, object? value)
        {
            if (value == null || !ModdablePropertyTypes.Any(x => x == value.GetType()))
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

        internal static PropertyInfo[] GetModdableProperties(this RpgObject context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsModdableProperty)
                .ToArray();
        }

        internal static TR? ExecuteFunction<TR>(this object? obj, string method) => obj.ExecuteFunction<TR>(method, null);
        internal static TR? ExecuteFunction<T1, TR>(this object? obj, string method, T1? arg1) => obj.ExecuteFunction<TR>(method, new object?[] { arg1 });
        internal static TR? ExecuteFunction<T1, T2, TR>(this object? obj, string method, T1? arg1, T2? arg2) => obj.ExecuteFunction<TR>(method, new object?[] { arg1, arg2 });
        internal static TR? ExecuteFunction<T1, T2, T3, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3) => obj.ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3 });
        internal static TR? ExecuteFunction<T1, T2, T3, T4, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4) => obj.ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3, arg4 });
        internal static TR? ExecuteFunction<T1, T2, T3, T4, T5, TR>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5) => obj.ExecuteFunction<TR>(method, new object?[] { arg1, arg2, arg3, arg4, arg5 });

        internal static TR? ExecuteFunction<TR>(this object? obj, string method, object?[]? args = null)
        {
            var methodInfo = obj.GetMethodInfo(method);

            var res = methodInfo.IsStatic
                ? (TR?)methodInfo?.Invoke(null, args)
                : (TR?)methodInfo?.Invoke(obj, args);

            return res;
        }

        internal static void ExecuteAction(this object? obj, string method) => obj.ExecuteAction(method);
        internal static void ExecuteAction<T1>(this object? obj, string method, T1? arg1) => obj.ExecuteAction(method, new object?[] { arg1 });
        internal static void ExecuteAction<T1, T2>(this object? obj, string method, T1? arg1, T2? arg2) => obj.ExecuteAction(method, new object?[] { arg1, arg2 });
        internal static void ExecuteAction<T1, T2, T3>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3) => obj.ExecuteAction(method, new object?[] { arg1, arg2, arg3 });
        internal static void ExecuteAction<T1, T2, T3, T4>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4) => obj.ExecuteAction(method, new object?[] { arg1, arg2, arg3, arg4 });
        internal static void ExecuteAction<T1, T2, T3, T4, T5>(this object? obj, string method, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5) => obj.ExecuteAction(method, new object?[] { arg1, arg2, arg3, arg4, arg5 });

        internal static void ExecuteAction(this object? obj, string method, object?[]? args = null)
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



        private static PropertyInfo? ModdableProperty(this object context, string prop)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == prop && x.IsModdableProperty());
        }

        private static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            if (!InScannableNamespace(propertyInfo.DeclaringType))
                return false;

            if (!ModdablePropertyTypes.Any(x => x == propertyInfo.PropertyType))
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

        private static IEnumerable<object> GetPropertyObjects(this object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            isEnumerable = false;

            if (propertyInfo.GetMethod?.Name == "get_Item" || IsExcludedType(propertyInfo.PropertyType))
                return Enumerable.Empty<object>();

            var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

            if (obj == null)
                return Enumerable.Empty<object>();

            var items = GetPropObjects(obj!, out isEnumerable);
            return items;
        }

        private static List<object> GetPropObjects(object? obj, out bool isEnumerable)
        {
            isEnumerable = false;

            var res = new List<object>();
            var items = new List<object?>();
            if (obj is IDictionary)
            {
                items = (obj as IDictionary)!.Values.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj is IEnumerable)
            {
                items = (obj as IEnumerable)!.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj != null)
                res.Add(obj);

            foreach (var item in items.Where(x => x != null))
                res.AddRange(GetPropObjects(item, out var _));

            return res;
        }

        private static bool IsExcludedType(Type type)
            => !type.IsPrimitive && ExcludedPropertyTypes.Any(type.IsAssignableTo);
    }
}
