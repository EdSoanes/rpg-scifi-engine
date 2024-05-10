using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public static class ModObjectExtensions
    {
        private class PropertyRef
        {
            public ModObject? Entity { get; set; }
            public string? Prop { get; set; }
        }

        public static void Merge(this List<ModObjectPropRef> target,ModObjectPropRef propRef)
        {
            if (!target.Any(x => x == propRef))
                target.Add(propRef);
        }

        public static void Merge(this List<ModObjectPropRef> target, IEnumerable<ModObjectPropRef> source)
        {
            foreach (var a in source)
                target.Merge(a);
        }

        public static void AddMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = PermanentMod.Create(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);
        }

        public static void AddMod<TEntity, TTarget, TSource>(this TEntity entity, Expression<Func<TEntity, TTarget>> targetExpr, Expression<Func<TEntity, TSource>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = PermanentMod.Create(entity, targetExpr, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);
        }

        public static void TriggerUpdate<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr)
            where TTarget : ModObject
        {
            var propRef = ModObjectPropRef.CreatePropRef(entity, targetExpr);
            entity.OnPropUpdated(propRef);
        }

        //public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Expression<Func<TEntity, Dice>> expression)
        //    where TEntity : ModdableObject
        //{
        //    var propertyRef = rootEntity.GetPropertyRef(expression);
        //    return !string.IsNullOrWhiteSpace(propertyRef.Prop)
        //        ? propertyRef.Entity?.GetPropValue(propertyRef.Prop)
        //        : null;
        //}

        //public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Modifier mod)
        //    where TEntity : ModdableObject
        //{
        //    var entity = rootEntity.FindModdableObject(mod.Source!.EntityId);
        //    return entity != null && !string.IsNullOrWhiteSpace(mod.Source.Prop)
        //        ? entity.GetPropValue(mod.Source.Prop)
        //        : null;
        //}

        //public static Modifier[]? GetMods<TEntity, TResult>(this TEntity rootEntity, Expression<Func<TEntity, TResult>> expression)
        //    where TEntity : ModObject
        //{
        //    var propertyRef = rootEntity.GetPropertyRef(expression);
        //    return propertyRef.Entity?.PropStore[propertyRef.Prop!]?.AllModifiers;
        //}

        //private static PropertyRef GetPropertyRef<T, TResult>(this T rootEntity, Expression<Func<T, TResult>> expression)
        //    where T : ModObject
        //{
        //    var memberExpression = expression.Body as MemberExpression;
        //    if (memberExpression == null)
        //        throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

        //    var pathSegments = new List<string>();

        //    //Get the prop name
        //    var prop = memberExpression.Member.Name;

        //    while (memberExpression != null)
        //    {
        //        memberExpression = memberExpression.Expression as MemberExpression;
        //        if (memberExpression != null)
        //            pathSegments.Add(memberExpression.Member.Name);
        //    }

        //    pathSegments.Reverse();
        //    var path = string.Join(".", pathSegments);
        //    var entity = rootEntity.PropertyValue<ModObject>(path);

        //    return new PropertyRef
        //    {
        //        Entity = entity,
        //        Prop = prop
        //    };
        //}
    }

}
