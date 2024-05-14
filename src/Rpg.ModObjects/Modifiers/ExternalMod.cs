using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class ExternalMod : Mod
    {
        public ExternalMod()
        {
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.External();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<ExternalMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(ExternalMod);

            return mod;
        }
    }

    public static class ExternalModExtensions
    {
        public static ModSet<T> Add<T, T1>(this ModSet<T> modSet, T entity, Expression<Func<T, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where T : ModObject
        {
            var mod = ExternalMod.Create(entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
