using System.Linq.Expressions;
using Rpg.Sys.Moddable;

namespace Rpg.Sys.Modifiers
{
    public class BaseOverrideModifier : Modifier
    {
        public BaseOverrideModifier()
        {
            ModifierType = ModifierType.BaseOverride;
            ModifierAction = ModifierAction.Replace;
        }

        public static Modifier Create<TEntity, T1>(TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
                => _Create<BaseOverrideModifier, TEntity, T1, TEntity, T1>(null, null, dice, null, entity, targetExpr, diceCalcExpr);
    }
}
