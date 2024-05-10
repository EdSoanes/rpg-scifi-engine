using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class BaseOverrideMod : Mod
    {
        public BaseOverrideMod()
        {
            ModifierType = ModType.BaseOverride;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<BaseOverrideMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(BaseOverrideMod);

            return mod;
        }
    }
}
