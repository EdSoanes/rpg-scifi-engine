using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace Rpg.ModObjects
{
    public static class ModObjectExtensions
    {
        private class PropertyRef
        {
            public ModObject? Entity { get; set; }
            public string? Prop { get; set; }
        }

        public static void Merge(this List<ModPropRef> target, ModPropRef propRef)
        {
            if (!target.Any(x => x == propRef))
                target.Add(propRef);
        }

        public static void Merge(this List<ModPropRef> target, IEnumerable<ModPropRef> source)
        {
            foreach (var a in source)
                target.Merge(a);
        }

        public static void AddBaseOverrideMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = BaseOverrideMod.Create(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);
        }

        public static void TriggerUpdate<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr)
            where TTarget : ModObject
        {
            var propRef = ModPropRef.CreatePropRef(entity, targetExpr);
            entity.TriggerUpdate(propRef);
        }

        public static ModPropDescription Describe<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : ModObject
        {
            var propRef = ModPropRef.CreatePropRef(entity, targetExpr);
            return entity.Describe(propRef);
        }
    }
}
