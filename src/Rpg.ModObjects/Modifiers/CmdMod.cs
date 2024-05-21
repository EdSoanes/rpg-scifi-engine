using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class CmdMod : Mod
    {
        public CmdMod()
        {
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.External();
        }

        public static Mod Create<TTarget, TTargetValue>(string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<CmdMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = $"{name}.{entity.Id}";

            return mod;
        }

        public static Mod Create<TTarget, TSource, TSourceValue>(string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<CmdMod, TTarget, TSource, TSourceValue>(target, targetProp, source, sourceExpr, diceCalcExpr);
            mod.Name = $"{name}.{target.Id}";

            return mod;
        }
    }

    public static class CmdModExtensions
    {
        public static ModSet<T> Add<T, T1>(this ModSet<T> modSet, string name, T entity, Expression<Func<T, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where T : ModObject
        {
            var mod = CmdMod.Create(name, entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet<TTarget> Add<TTarget, TSource, TSourceValue>(this ModSet<TTarget> modSet, string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = CmdMod.Create(name, target, targetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
