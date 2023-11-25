using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Meta
{
    public static class PropEngine
    {
        public static Dice Evaluate(this Entity entity, string prop)
        {
            var meta = entity.Meta();
            var mods = meta?.GetMods(prop)?.ToList() ?? new List<Modifier>();

            Dice dice = "0";
            mods.ForEach(x => dice += x.Evaluate());

            return dice;
        }

        public static int Resolve(this Entity entity, string prop)
        {
            var meta = entity.Meta();
            var mods = meta?.GetMods(prop);

            Dice dice = mods != null
                ? string.Concat(mods.Select(x => x.Evaluate()))
                : "0";

            //TODO: We need to store the result so we don't get different resolutions each time this is called.
            return dice.Roll();
        }

        public static string[] Describe(this Entity entity, string prop)
        {
            var meta = entity.Meta();
            var mods = meta?.GetMods(prop);

            var res = mods?
                .Select(x => $"{x.Source?.Prop ?? "Set"} => {x.Target.Prop} {x.Dice}")
                .ToArray() ?? new string[0];

            return res;
        }

        //public static bool AddMod(this Entity entity, Modifier mod)
        //{
        //    var meta = mod.Target.Id.Meta();
        //    return meta.AddMod(mod);
        //}

        //public static bool AddMod(this Guid id, Modifier mod)
        //{
        //    var meta = id.Meta();
        //    return meta.AddMod(mod);
        //}

        public static Modifier[] RemoveMods(this Entity entity)
        {
            var meta = entity.Meta();
            return meta?.RemoveMods() ?? new Modifier[0];
        }

        public static Modifier[] RemoveMods(this Entity entity, string prop)
        {
            var meta = entity.Meta();
            return meta?.RemoveMods(prop) ?? new Modifier[0];
        }

        public static Modifier[] RemoveMods(this Guid id, string prop)
        {
            var meta = id.Meta();
            return meta?.RemoveMods(prop) ?? new Modifier[0];
        }

        public static Modifier[] RemoveModsByName(Guid id, string name)
        {
            var meta = id.Meta();
            return meta?.RemoveModsByName(name) ?? new Modifier[0];
        }

        public static Modifier[] RemoveModsByName(this Entity entity, string name)
        {
            var meta = entity.Meta();
            return meta?.RemoveModsByName(name) ?? new Modifier[0];
        }

        public static void AddMod<TSource, TTarget, T>(this TSource source, TTarget target, string modifierName, Expression<Func<TSource, T>> sourceExpr, Expression<Func<TTarget, T>> targetExpr)
            where TSource : Entity
            where TTarget : Entity
        {
            var mod = source.Modifies(target, modifierName, sourceExpr, targetExpr);
            target.Meta()?.AddMod(mod);
        }

        public static Modifier Modifies<TSource, TTarget, T>(this TSource source, TTarget target, string modifierName, Expression<Func<TSource, T>> sourceExpr, Expression<Func<TTarget, T>> targetExpr)
            where TSource : Entity
            where TTarget : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(target, targetExpr);
            return new Modifier(modifierName, sourceLocator, targetLocator);
        }

        public static void AddMod<TSource, TTarget, T>(this TSource source, TTarget target, Expression<Func<TSource, T>> sourceExpr, Expression<Func<TTarget, T>> targetExpr)
            where TSource : Entity
            where TTarget : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(target, targetExpr);
            target.Meta()?.AddMod(new Modifier(targetLocator.Prop, sourceLocator, targetLocator));
        }

        public static Modifier Modifies<TSource, TTarget, T>(this TSource source, TTarget target, Expression<Func<TSource, T>> sourceExpr, Expression<Func<TTarget, T>> targetExpr)
            where TSource : Entity
            where TTarget : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(target, targetExpr);
            return new Modifier(targetLocator.Prop, sourceLocator, targetLocator);
        }

        public static void AddMod<TTarget, T1>(this TTarget target, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            target.Meta()?.AddMod(new Modifier(modifierName, dice, targetLocator));
        }

        public static Modifier Modifies<TTarget, T1>(this TTarget target, string modifierName, Dice dice, Expression<Func<TTarget, T1>> targetExpr)
            where TTarget : Entity
        {
            var targetLocator = GetModLocator(target, targetExpr);
            return new Modifier(modifierName, dice, targetLocator);
        }

        public static void AddMod<TSource, T1, T2>(this TSource source, string modifierName, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            source.Meta()?.AddMod(new Modifier(modifierName, sourceLocator, targetLocator));
        }

        public static Modifier Modifies<TSource, T1, T2>(this TSource source, string modifierName, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            return new Modifier(modifierName, sourceLocator, targetLocator);
        }

        public static void AddMod<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            source.Meta()?.AddMod(new Modifier(targetLocator.Prop, sourceLocator, targetLocator));
        }

        public static Modifier Modifies<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
            where TSource : Entity
        {
            var sourceLocator = GetModLocator(source, sourceExpr, true);
            var targetLocator = GetModLocator(source, targetExpr);
            return new Modifier(targetLocator.Prop, sourceLocator, targetLocator);
        }

        public static void AddMod<TSource, T1, T2>(this TSource source, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>> diceCalc)
            where TSource : Entity
        {
            var mod = source.Modifies(sourceExpr, targetExpr, diceCalc);
            source.Meta()?.AddMod(mod);
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

            var pathEntity = entity.GetEntityFromPath(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
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
    }
}
