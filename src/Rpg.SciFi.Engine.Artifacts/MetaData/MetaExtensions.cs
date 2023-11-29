using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public static class MetaExtensions
    {
        public static MetaEntity? MetaData(this Entity entity)
        {
            return Meta.MetaEntities?.SingleOrDefault(x => x.Id == entity.Id && x.Type == entity.GetType().Name);
        }

        public static MetaEntity MetaData(this Guid id)
        {
            if (Meta.MetaEntities == null)
                throw new InvalidOperationException($"{nameof(Meta)} not initialized");

            return Meta.MetaEntities.Single(x => x.Id == id);
        }

        public static Dice Evaluate(this Entity entity, string prop)
        {
            var meta = entity.MetaData();
            var mods = meta?.GetMods(prop)?.ToList() ?? new List<Modifier>();

            Dice dice = "0";
            mods.ForEach(x => dice += x.Evaluate());

            return dice;
        }

        public static int Resolve(this Entity entity, string prop)
        {
            var meta = entity.MetaData();
            var mods = meta?.GetMods(prop);

            Dice dice = mods != null
                ? string.Concat(mods.Select(x => x.Evaluate()))
                : "0";

            //TODO: We need to store the result so we don't get different resolutions each time this is called.
            return dice.Roll();
        }

        public static string[] Describe(this Entity entity, string prop)
        {
            var meta = entity.MetaData();
            var mods = meta?.GetMods(prop);

            var res = mods?
                .Select(x => $"{x.Source?.Prop ?? "Set"} => {x.Target.Prop} {x.Dice}")
                .ToArray() ?? new string[0];

            return res;
        }

        public static Modifier[] RemoveMods(this Entity entity)
        {
            var meta = entity.MetaData();
            return meta?.RemoveMods() ?? new Modifier[0];
        }

        public static Modifier[] RemoveMods(this Entity entity, string prop)
        {
            var meta = entity.MetaData();
            return meta?.RemoveMods(prop) ?? new Modifier[0];
        }

        public static Modifier[] RemoveMods(this Guid id, string prop)
        {
            var meta = id.MetaData();
            return meta?.RemoveMods(prop) ?? new Modifier[0];
        }

        public static Modifier[] RemoveModsByName(this Guid id, string name)
        {
            var meta = id.MetaData();
            return meta?.RemoveModsByName(name) ?? new Modifier[0];
        }
        public static Modifier[] RemoveModsByName(this Entity entity, string name)
        {
            var meta = entity.MetaData();
            return meta?.RemoveModsByName(name) ?? new Modifier[0];
        }

        public static void AddBaseMod<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where TSource : Entity
        => source.AddMod(ModType.Base, sourceExpr, source, targetExpr, diceCalc);

        public static void AddMod<TSource, T1, T2>(this TSource source, ModType type, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where TSource : Entity
                => source.AddMod(type, sourceExpr, source, targetExpr, diceCalc);

        public static void AddMod<TSource, TTarget, T1, T2>(
            this TSource source,
            ModType type,
            Expression<Func<TSource, T1>> sourceExpr,
            TTarget target,
            Expression<Func<TTarget, T2>> targetExpr,
            Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where TSource : Entity
            where TTarget : Entity
        {
            var sourceLocator = source.CreateModLocator(sourceExpr, true);
            var targetLocator = target.CreateModLocator(targetExpr);
            var calc = diceCalc != null ? GetCalculationMethod(diceCalc) : null;

            targetLocator.Id.MetaData()
                ?.AddMod(new Modifier(type, sourceLocator, targetLocator, calc));
        }

        public static void AddMod<TTarget, T1>(this TTarget target, ModType type, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = target.CreateModLocator(targetExpr);
            targetLocator.Id.MetaData()?.AddMod(new Modifier(modifierName, type, dice, targetLocator));
        }

        public static Modifier Modifies<TTarget, T1>(this TTarget target, ModType type, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = target.CreateModLocator(targetExpr);
            return new Modifier(modifierName, type, dice, targetLocator);
        }

        public static T? PropertyValue<T>(this Entity entity, string path)
        {
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

        public static MetaModLocator CreateModLocator<T, TResult>(this T entity, Expression<Func<T, TResult>> expression, bool source = false)
            where T : Entity
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();
            var prop = memberExpression.Member.Name;

            var moddable = memberExpression.Member.GetCustomAttribute<ModdableAttribute>() != null;
            if (!moddable && !source)
                throw new ArgumentException($"Invalid path. Property {memberExpression.Member.Name} must have the attribute {nameof(ModdableAttribute)}");

            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();
            var path = string.Join(".", pathSegments);

            var pathEntity = entity.PropertyValue<Entity>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
            var locator = new MetaModLocator(pathEntity.Id, prop);
            return locator;
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

        private static string GetCalculationMethod<T>(Expression<Func<Func<T, T>>> expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MemberInfo)methodInfoExpression.Value!;
            return $"{methodInfo.DeclaringType!.Name}.{methodInfo.Name}";
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
                if (Meta.Context != null)
                    type = Assembly.GetAssembly(Meta.Context.GetType())!
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
    }
}
