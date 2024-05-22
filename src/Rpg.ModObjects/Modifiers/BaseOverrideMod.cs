using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class BaseOverrideMod : Mod
    {
        [JsonConstructor] private BaseOverrideMod() { }

        public BaseOverrideMod(ModPropRef targetPropRef)
            : this(nameof(BaseOverrideMod), targetPropRef)
        {
        }

        public BaseOverrideMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.BaseOverride;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class BaseOverrideModExtensions
    {
        public static Mod AddBaseOverrideMod<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<BaseOverrideMod, TTarget, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            entity.AddMod(mod);
            return mod;
        }
    }
}
