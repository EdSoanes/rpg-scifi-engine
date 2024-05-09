using System.Linq.Expressions;

namespace Rpg.Sys.Moddable.Modifiers
{
    public class DamageMod : Mod
    {
        public DamageMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
            Duration.SetWhenPropertyZero();
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
}
