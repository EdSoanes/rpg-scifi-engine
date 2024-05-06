using System.Linq.Expressions;
using Rpg.Sys.Moddable;

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

        public static Modifier Create(ModObject entity, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => _CreateByPath<BaseModifier>(entity, ModNames.BaseValue, dice, targetPropPath, diceCalcExpr);
    }
}
