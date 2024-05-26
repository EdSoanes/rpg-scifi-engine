using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class PermanentMod : Mod
    {
        [JsonConstructor] private PermanentMod() { }

        public PermanentMod(ModPropRef targetPropRef)
            : this(nameof(PermanentMod), targetPropRef)
        {
        }

        public PermanentMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Accumulate;
            Duration = ModDuration.Permanent();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class PermanentModExtensions
    {
        public static Mod AddPermanentMod<TEntity>(this TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity>(entity, targetProp, dice, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddPermanentMod<TEntity, TTargetValue, TSourceValue>(this TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddPermanentMod<TEntity, TTargetValue>(this TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue>(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddPermanentMod<TEntity, TTarget, TTargetValue, TSourceValue>(this TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<PermanentMod, TEntity, TTargetValue, TEntity, TSourceValue>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }
    }
}
