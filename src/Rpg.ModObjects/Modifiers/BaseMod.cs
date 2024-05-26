using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class BaseMod : Mod
    {
        [JsonConstructor] private BaseMod() { }

        public BaseMod(ModPropRef targetPropRef)
            : this(nameof(BaseMod), targetPropRef)
        {
        }

        public BaseMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Base;
            ModifierAction = ModAction.Accumulate;
            Duration = ModDuration.Permanent();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class BaseModExtensions
    {
        public static TTarget AddBaseMod<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<BaseMod, TTarget, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);

            return entity;
        }

        public static TTarget AddBaseMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<BaseMod, TTarget, TTargetValue, TTarget, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);

            return entity;
        }
    }
}
