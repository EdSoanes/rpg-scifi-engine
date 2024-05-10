using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class CostMod : Mod
    {
        public CostMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
        }

        public override void OnAdd(int turn)
        {
            Duration = ModDuration.OnNewTurn(turn);
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<CostMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(CostMod);

            return mod;
        }
    }
}
