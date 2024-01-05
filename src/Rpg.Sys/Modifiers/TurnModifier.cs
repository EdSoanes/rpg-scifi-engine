using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class TurnModifier : Modifier
    {
        public TurnModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Accumulate;
            StartTurn = RemoveTurn.EndOfTurn;
            EndTurn = RemoveTurn.EndOfTurn;
        }

        public override ModifierExpiry SetExpiry(int turn)
        {
            if (turn < 1)
            {
                Expiry = ModifierExpiry.Remove;
                return Expiry;
            }

            if (EndTurn == RemoveTurn.EndOfTurn)
            {
                StartTurn = turn;
                EndTurn = turn;
            }

            Expiry = ModifierExpiry.Expired;
            return Expiry;
        }

        public static Modifier Create<TEntity, T1>(TEntity target, string name, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<TurnModifier, TEntity, T1, TEntity, T1>(null, name, dice, null, target, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(TEntity target, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<TurnModifier, TEntity, T1, TEntity, T1>(null, ModNames.Turn, dice, null, target, targetExpr, diceCalcExpr);
    }
}
