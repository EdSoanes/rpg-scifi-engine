using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class EntityExtensions
    {

        //public static Dice Evaluate<T, TResult>(this T entity, Expression<Func<T, TResult>> expression) where T : Entity
        //{
        //    var moddableProperty = entity.ToModdableProperty(expression);
        //    return entity.Evaluate(moddableProperty.Prop!);
        //}

        public static string[] Describe<T, TResult>(this T entity, Expression<Func<T, TResult>> expression) where T : Entity
        {
            var moddableProperty = PropRef.FromPath(entity, expression);
            return entity.Describe(moddableProperty.Prop);
        }

        //public static Modifier ModByPath<T1>(
        //    this Entity entity,
        //    string name,
        //    Dice dice,
        //    string targetPropPath,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //{
        //    var tgt = entity.ToModdableProperty(targetPropPath);
        //    var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

        //    return new BaseModifier(name, dice, tgt, calc);
        //}

        //public static Modifier Mod<TSource, T1>(
        //    this TSource entity,
        //    Dice dice,
        //    Expression<Func<TSource, T1>> targetExpr,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //    where TSource : Entity
        //{
        //    var tgt = entity.ToModdableProperty(targetExpr);
        //    var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

        //    return new BaseModifier(tgt!.Prop!, dice, tgt, calc);
        //}

        //public static Modifier Mod<TSource, T1>(
        //    this TSource entity, 
        //    string name,
        //    Dice dice,
        //    Expression<Func<TSource, T1>> targetExpr,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //    where TSource : Entity
        //{
        //    var tgt = entity.ToModdableProperty(targetExpr);
        //    var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

        //    return new BaseModifier(name, dice, tgt, calc);
        //}

        //public static Modifier Mod<TSource, T1, T2>(
        //    this TSource entity,
        //    Expression<Func<TSource, T1>> sourceExpr,
        //    Expression<Func<TSource, T2>> targetExpr,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //    where TSource : Entity
        //    => entity.Mod(null, sourceExpr, entity, targetExpr, diceCalcExpr);

        //public static Modifier Mod<TSource, TTarget, T1, T2>(
        //    this TSource entity,
        //    Expression<Func<TSource, T1>> sourceExpr,
        //    TTarget target,
        //    Expression<Func<TTarget, T2>> targetExpr,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //    where TSource : Entity
        //    where TTarget : Entity
        //    => entity.Mod(null, sourceExpr, target, targetExpr, diceCalcExpr);

        //private static Modifier Mod<TSource, TTarget, T1, T2>(
        //    this TSource entity,
        //    string? name,
        //    Expression<Func<TSource, T1>> sourceExpr,
        //    TTarget target,
        //    Expression<Func<TTarget, T2>> targetExpr,
        //    Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        //    where TSource : Entity
        //    where TTarget : Entity
        //{
        //    var src = entity.ToModdableProperty(sourceExpr, true);
        //    var tgt = target.ToModdableProperty(targetExpr);
        //    var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

        //    name ??= src.Prop;
        //    return new BaseModifier(name, src, tgt, calc);
        //}

        //public static ModdableProperty ToModdableProperty(this Entity entity, string propPath, bool source = false)
        //{
        //    var parts = propPath.Split('.');
        //    var path = string.Join(".", parts.Take(parts.Length - 1));
        //    var prop = parts.Last();

        //    var pathEntity = entity.PropertyValue<Entity>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
        //    if (!source && !pathEntity.IsModdableProperty(prop))
        //        throw new ArgumentException($"Invalid path. Property {prop} must have the attribute {nameof(ModdableAttribute)}");

        //    var locator = new ModdableProperty(entity.Id, pathEntity.Id, pathEntity.GetType().Name, prop, null);
        //    return locator;
        //}

        //public static ModdableProperty ToModdableProperty<T, TResult>(this T entity, Expression<Func<T, TResult>> expression, bool source = false)
        //    where T : Entity
        //{
        //    var memberExpression = expression.Body as MemberExpression;
        //    if (memberExpression == null)
        //        throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

        //    var pathSegments = new List<string>();

        //    //Get the prop name
        //    var prop = memberExpression.Member.Name;
        //    var moddable = memberExpression.Member.GetCustomAttribute<ModdableAttribute>() != null;
        //    if (!moddable && !source)
        //        throw new ArgumentException($"Invalid path. Property {memberExpression.Member.Name} must have the attribute {nameof(ModdableAttribute)}");

        //    while (memberExpression != null)
        //    {
        //        memberExpression = memberExpression.Expression as MemberExpression;
        //        if (memberExpression != null)
        //            pathSegments.Add(memberExpression.Member.Name);
        //    }

        //    pathSegments.Reverse();
        //    var path = string.Join(".", pathSegments);
        //    var pathEntity = entity.PropertyValue<Entity>(path);

        //    var locator = new ModdableProperty(entity.Id, pathEntity!.Id, pathEntity!.GetType().Name, prop, null);
        //    return locator;
        //}
    }
}
