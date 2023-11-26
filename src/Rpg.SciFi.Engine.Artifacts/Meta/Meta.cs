using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;
using System.Reflection;

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

        public static void AddMod<TSource, TTarget, T>(this TSource source, Expression<Func<TSource, T>> sourceExpr, TTarget target, Expression<Func<TTarget, T>> targetExpr)
            where TSource : Entity
            where TTarget : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(target, targetExpr);
            target.MetaData()?.AddMod(new Modifier(targetLocator.Prop, sourceLocator, targetLocator));
        }

        public static void AddMod<TTarget, T1>(this TTarget target, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            target.MetaData()?.AddMod(new Modifier(modifierName, dice, targetLocator));
        }

        public static Modifier Modifies<TTarget, T1>(this TTarget target, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            return new Modifier(modifierName, dice, targetLocator);
        }

        public static void AddMod<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            source.MetaData()?.AddMod(new Modifier(targetLocator.Prop, sourceLocator, targetLocator));
        }

        public static void AddMod<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>> diceCalc)
            where TSource : Entity
        {
            var mod = source.Modifies(sourceExpr, targetExpr, diceCalc);
            source.MetaData()?.AddMod(mod);
        }

        public static Modifier Modifies<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>> diceCalc)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            var calc = GetDiceCalculation(diceCalc);
            return new Modifier(targetLocator.Prop, sourceLocator, targetLocator);
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

        public static string GetDiceCalculation(Expression<Func<Func<Dice, Dice>>> diceCalc)
        {
            var memberExpression = diceCalc.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {diceCalc.Name} not a member expression");

            var pathSegments = new List<string>();
            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();

            return string.Join(".", pathSegments);
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
