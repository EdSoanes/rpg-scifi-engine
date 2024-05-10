using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class TurnMod : Mod
    {
        public TurnMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Accumulate;
        }

        public override void OnAdd(int turn)
        {
            Duration = ModDuration.OnNewTurn(turn);
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(TurnMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue>(string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = name;

            return mod;
        }
    }
}
