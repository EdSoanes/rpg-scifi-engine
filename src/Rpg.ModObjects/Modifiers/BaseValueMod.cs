using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class BaseValueMod : Mod
    {
        public BaseValueMod()
        {
            ModifierType = ModType.Base;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
        }

        public static Mod Create<TTarget>(TTarget entity, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<BaseValueMod, TTarget>(entity, prop, value, diceCalcExpr);
            mod.Name = nameof(BaseValueMod);

            return mod;
        }
    }
}
