using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    internal class BaseInitMod : Mod
    {
        public BaseInitMod()
        {
            ModifierType = ModType.BaseInit;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
        }

        public static Mod Create<TTarget>(TTarget entity, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<BaseInitMod, TTarget>(entity, prop, value, diceCalcExpr);
            mod.Name = prop;

            return mod;
        }
    }
}
