using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class SumMod : Mod
    {
        [JsonConstructor] private SumMod() { }

        public SumMod(ModPropRef targetPropRef)
            : this(nameof(SumMod), targetPropRef)
        {
        }

        public SumMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.OnValueZero();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class SumModExtensions
    {
        public static Mod AddSumMod<TEntity>(this TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<SumMod, TEntity>(entity, targetProp, dice, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddSumMod<TEntity, TTargetValue>(this TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<SumMod, TEntity, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddSumMod<TTarget, TTargetValue, TSource, TSourceValue>(this TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<SumMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            target.AddMod(mod);

            return mod;
        }
    }
}
