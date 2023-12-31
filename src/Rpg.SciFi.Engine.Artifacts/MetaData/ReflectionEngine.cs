using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public static class ReflectionEngine
    {
        public static MetaEntity CreateMetaEntity(this Entity entity)
        {
            var metaEntity = new MetaEntity
            {
                Entity = entity,
                Class = entity.GetEntityClass(),
                SetupMethods = entity.GetSetupMethods(),
                AbilityMethods = entity.GetAbilityMethods()
            };

            foreach (var propertyInfo in entity.MetaProperties())
            {
                if (propertyInfo.IsModdableProperty())
                    metaEntity.ModdableProperties.Add(propertyInfo.Name);
            }

            return metaEntity;
        }

        public static T? PropertyValue<T>(this Entity? entity, string path)
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
                var propInfo = res.MetaProperty(part);
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

        public static void PropertyValue<T>(this object? entity, string path, T? value)
        {
            if (entity != null && !string.IsNullOrEmpty(path))
            {
                var parts = path.Split('.');
                if (parts.Length > 0)
                {
                    object? res = entity;

                    var prop = parts.Last();
                    if (parts.Length > 1)
                    {
                        foreach (var part in parts.Take(parts.Length - 1))
                        {
                            var propInfo = res.MetaProperty(part);
                            res = propInfo?.GetValue(res, null);
                            if (res == null)
                                break;
                        }
                    }
                    var propertyInfo = res?.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (propertyInfo?.SetMethod != null)
                        propertyInfo?.SetValue(res, value);
                    else
                        return;
                }
            }
        }

        internal static bool IsModdableProperty(this Entity entity, string prop)
        {
            return entity.GetType().GetProperty(prop)?.IsModdableProperty() ?? false;
        }

        internal static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<ModdableAttribute>();
            return attr != null;
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
                type = Assembly.GetAssembly(typeof(EntityManager<Entity>))!
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

        internal static string GetEntityClass(this object obj)
        {
            if (obj.GetType().IsAssignableTo(typeof(Artifact)))
                return nameof(Artifact);

            if (obj.GetType().IsAssignableTo(typeof(Entity)))
                return nameof(Entity);

            return nameof(Object);
        }

        internal static string[] GetSetupMethods(this object obj)
        {
            var setupMethods = obj.GetType().GetMethods()
                .Where(x => x.IsSetupMethod())
                .Select(x => x.Name)
                .ToArray();

            return setupMethods;
        }

        internal static MetaAction[] GetAbilityMethods(this object context)
        {
            var metaAbilityMethods = new List<MetaAction>();

            foreach (var methodInfo in context.GetType().GetMethods())
            {
                var attr = methodInfo.GetCustomAttribute<AbilityAttribute>();
                if (attr == null || methodInfo.ReturnType != typeof(Turns.Action))
                    continue;

                var metaAbilityMethod = new MetaAction
                {
                    Name = methodInfo.Name
                };

                var inputAttrs = methodInfo.GetCustomAttributes<InputAttribute>();
                foreach (var parameter in methodInfo.GetParameters())
                {
                    var inputAttr = inputAttrs.FirstOrDefault(x => x.Param == parameter.Name);
                    if (inputAttr == null)
                        throw new ArgumentException($"{methodInfo.Name} missing matching Input attribute");

                    var metaActionInput = new MetaActionInput
                    {
                        Name = inputAttr.Param,
                        BindsTo = inputAttr.BindsTo,
                        InputSource = inputAttr.InputSource
                    };

                    metaAbilityMethod.Inputs.Add(metaActionInput);
                }

                metaAbilityMethods.Add(metaAbilityMethod);
            }

            return metaAbilityMethods.ToArray();
        }

        internal static PropertyInfo? MetaProperty(this object context, string prop)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == prop && x.IsMetaProperty());
        }

        internal static PropertyInfo[] MetaProperties(this object context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsMetaProperty)
                .ToArray();
        }

        internal static bool IsSetupMethod(this MethodInfo method)
        {
            return method.GetCustomAttribute<SetupAttribute>() != null
                && method.ReturnType.IsAssignableTo(typeof(Modifier[]))
                && !(method.GetParameters()?.Any() ?? false);
        }

        internal static bool IsMetaProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily)
                && !(propertyInfo.PropertyType.Namespace!.StartsWith("System") && propertyInfo.PropertyType.Name.StartsWith("Func"))
                && !(propertyInfo.PropertyType?.IsAssignableTo(typeof(IEntityManager)) ?? false)
                && !(propertyInfo.DeclaringType?.IsAssignableTo(typeof(MetaEntity)) ?? false);
        }

        internal static IEnumerable<object> PropertyObjects(this object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            try
            {
                isEnumerable = false;
                if (propertyInfo.IsModdableProperty())
                    return Enumerable.Empty<object>();

                var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                if (obj == null || obj is string || obj.GetType().IsPrimitive || obj is Guid || obj is Dice)
                    return Enumerable.Empty<object>();

                if (obj is IEnumerable)
                {
                    isEnumerable = true;
                    return (obj as IEnumerable)!.Cast<object>();
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
