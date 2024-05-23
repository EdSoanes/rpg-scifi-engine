using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ExternalMod : Mod
    {
        [JsonConstructor] private ExternalMod() { }

        public ExternalMod(ModPropRef targetPropRef)
            : this(nameof(ExternalMod), targetPropRef)
        {
        }

        public ExternalMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Accumulate;
            Duration = ModDuration.External();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class ExternalModExtensions
    {
        public static ModSet AddExternalMod<TEntity>(this ModSet modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(entity, targetProp, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(entity, targetProp, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(target, targetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTargetValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet AddExternalMod<TEntity, TTarget, TTargetValue, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
