using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class DamageMod : Mod
    {
        public DamageMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.OnValueZero();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value)
            where TTarget : ModObject
        {
            var mod = Create<DamageMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, () => DiceCalculations.Minus);
            mod.Name = nameof(DamageMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<DamageMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, () => DiceCalculations.Minus);
            mod.Name = nameof(DamageMod);

            return mod;
        }
    }

    public static class DamageModExtensions
    {
        public static void AddDamageMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice)
            where TEntity : ModObject
        {
            var mod = DamageMod.Create(entity, targetExpr, dice);
            entity.AddMod(mod);
        }

        public static void AddDamageMod<TTarget, TTargetValue, TSource, TSourceValue>(this TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = DamageMod.Create(target, targetExpr, source, sourceExpr);
            target.AddMod(mod);
        }
    }
}
