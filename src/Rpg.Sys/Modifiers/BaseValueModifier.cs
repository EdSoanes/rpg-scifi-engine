using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class BaseValueModifier : Modifier
    {
        public const string BaseName = "BaseValue";

        public BaseValueModifier() 
        {
            Name = BaseName;
            ModifierType = ModifierType.Base;
            ModifierAction = ModifierAction.Replace;
        }

        public static Modifier Create(ModdableObject entity, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => _CreateByPath<BaseModifier>(entity, ModNames.BaseValue, dice, targetPropPath, diceCalcExpr);
    }
}
