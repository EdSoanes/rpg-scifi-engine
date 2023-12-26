using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class CostModifier : Modifier
    {
        public CostModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            RemoveOnTurn = RemoveTurn.This;
            IsPermanent = false;
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : Entity
                => _Create<CostModifier, TEntity, T1, TEntity, T1>(null, ModNames.Cost, dice, null, target, targetExpr, diceCalcExpr);
    }
}
