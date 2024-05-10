using System.Linq.Expressions;

namespace Rpg.Sys.Moddable.Modifiers
{
    public class HealingMod : Mod
    {
        public HealingMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.OnValueZero();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value)
            where TTarget : ModObject
        {
            var mod = Create<HealingMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value);
            mod.Name = nameof(DamageMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<HealingMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr);
            mod.Name = nameof(DamageMod);

            return mod;
        }
    }
}
