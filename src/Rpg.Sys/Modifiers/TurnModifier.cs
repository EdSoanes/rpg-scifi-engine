using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class TurnModifier : Modifier
    {
        public TurnModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Accumulate;
            Duration.SetWhenEndOfTurn();
        }

        public override void OnAdd(int turn)
        {
            Duration.SetTurn(turn);
        }

        public static Modifier Create<TEntity, T1>(TEntity target, string name, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<TurnModifier, TEntity, T1, TEntity, T1>(null, name, dice, null, target, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(TEntity target, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<TurnModifier, TEntity, T1, TEntity, T1>(null, ModNames.Turn, dice, null, target, targetExpr, diceCalcExpr);
    }
}
