using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class PlayerModifier : Modifier
    {
        public PlayerModifier()
        {
            ModifierType = ModifierType.Player;
            ModifierAction = ModifierAction.Replace;
        }

        public static Modifier Create<TEntity, T1>(TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<PlayerModifier, TEntity, T1, TEntity, T1>(null, ModNames.Base, dice, null, entity, targetExpr, diceCalcExpr);
    }
}
