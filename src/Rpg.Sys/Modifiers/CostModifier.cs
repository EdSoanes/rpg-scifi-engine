using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class CostModifier : Modifier
    {
        public CostModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            EndTurn = RemoveTurn.EndOfTurn;
        }

        public override ModifierExpiry SetExpiry(int turn)
        {
            Expiry = ModifierExpiry.Expired;
            return Expiry;
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<CostModifier, TEntity, T1, TEntity, T1>(null, ModNames.Cost, dice, null, target, targetExpr, diceCalcExpr);
    }
}
