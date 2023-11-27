using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Rpg.SciFi.Engine.Artifacts.Meta
{
    public static class Meta
    {
        public static Entity? Context { get; private set; }
        public static MetaEntity[]? MetaEntities { get; private set; }

        public static List<object> _setups = new List<object>();

        public static void Initialize(Entity context)
        {
            Context = context;
            var meta = context.TraverseMetaGraph((metaEntity, path, propertyInfo) =>
            {
                if (propertyInfo.IsModdableProperty())
                    metaEntity.ModifiableProperties?.Add(propertyInfo.Name);
            });

            MetaEntities = meta
                .OrderBy(x => x.Path)
                .ToArray();
        }

        public static void Clear()
        {
            Context = null;
            MetaEntities = null;
            _setups.Clear();
        }

        public static string Serialize()
        {
            var entityJson = JsonConvert.SerializeObject(Context, Formatting.None);
            var metaJson = JsonConvert.SerializeObject(MetaEntities, Formatting.None);

            var sb = new StringBuilder();
            sb.AppendLine(entityJson);
            sb.AppendLine(metaJson);
            return sb.ToString();
        }

        public static void Deserialize<T>(string state)
            where T : Entity
        {
            var files = state.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (files.Length != 2)
                throw new ArgumentException("State should contain 2 files");

            Context = JsonConvert.DeserializeObject<T>(files[0]);
            MetaEntities = JsonConvert.DeserializeObject<MetaEntity[]>(files[1]);

            ReflectionEngine.TraverseMetaGraph(Context!, (e, _, _) =>
            {
                var metaEntity = MetaEntities!.Single(x => x.Id == e.Id);
                metaEntity.SetEntity(e.Entity!);
            });
        }

        public static MetaEntity? MetaData(this Entity entity)
        {
            return MetaEntities?.SingleOrDefault(x => x.Id == entity.Id && x.Type == entity.GetType().Name);
        }

        public static MetaEntity MetaData(this Guid id)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException($"{nameof(Artifacts.Meta.Meta)} not initialized");

            return MetaEntities.Single(x => x.Id == id);
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

        public static Modifier[] RemoveModsByName(Guid id, string name)
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
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(target, targetExpr);
            var calc = diceCalc != null ? GetCalculationMethod(diceCalc) : null;

            targetLocator.Id.MetaData()
                ?.AddMod(new Modifier(type, sourceLocator, targetLocator, calc));
        }

        public static void AddMod<TTarget, T1>(this TTarget target, ModType type, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            targetLocator.Id.MetaData()?.AddMod(new Modifier(modifierName, type, dice, targetLocator));
        }

        public static Modifier Modifies<TTarget, T1>(this TTarget target, ModType type, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            return new Modifier(modifierName, type, dice, targetLocator);
        }

        public static MetaModLocator GetModLocator<T, TResult>(Entity entity, Expression<Func<T, TResult>> expression, bool source = false)
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

        public static T RunCalculationMethod<T>(string method, T input, object? obj = null)
        {
            var parts = method.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException($"Specified method {method} is invalid");

            var cls = parts[0];
            var mthd = parts[1];
            var type = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(x => x.Name == cls);

            if (type == null)
                throw new ArgumentException($"Class in {method} does not exist");

            var methodInfo = type?.GetMethod(mthd);
            if (methodInfo == null)
                throw new ArgumentException($"Method in {method} does not exist");

            var res = methodInfo.IsStatic
                ? (T?)methodInfo?.Invoke(null, new object?[] { input })
                : (T?)methodInfo?.Invoke(obj, new object?[] { input });

            return res ?? default;
        }

        public static string GetCalculationMethod<T>(Expression<Func<Func<T, T>>> expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MemberInfo)methodInfoExpression.Value!;
            return $"{methodInfo.DeclaringType!.Name}.{methodInfo.Name}";
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
    }
}
