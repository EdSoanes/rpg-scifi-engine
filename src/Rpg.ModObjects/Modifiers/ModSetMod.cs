using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSetMod : Mod
    {
        public ModSetMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Accumulate;
            Duration = ModDuration.External();
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<ModSetMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = nameof(ModSetMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(string name, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<ModSetMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = name;

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<ModSetMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(ModSetMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue>(string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<ModSetMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = name;

            return mod;
        }
    }
}
