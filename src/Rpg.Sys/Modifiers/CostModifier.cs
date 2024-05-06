using System.Linq.Expressions;
using Rpg.Sys.Moddable;

namespace Rpg.Sys.Modifiers
{
    public class CostModifier : Modifier
    {
        public CostModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            Duration.SetWhenEndOfTurn();
        }

        public override void OnAdd(int turn)
        {
            Duration.SetOnTurn(turn);
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
                => _Create<CostModifier, TEntity, T1, TEntity, T1>(null, ModNames.Cost, dice, null, target, targetExpr, diceCalcExpr);
    }
}
