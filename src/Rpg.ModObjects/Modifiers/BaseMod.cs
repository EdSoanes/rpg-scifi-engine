using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class BaseMod : Mod
    {
        public BaseMod()
        {
            ModifierType = ModType.Base;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<BaseMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(BaseMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<BaseMod, TTarget, TTargetValue, TTarget, TSourceValue>(target, targetExpr, target, sourceExpr, diceCalcExpr);
            mod.Name = nameof(BaseMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<BaseMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = nameof(BaseMod);

            return mod;
        }
    }

    public static class BaseModExtensions
    {
        public static TEntity AddBaseMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = BaseMod.Create(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);

            return entity;
        }

        public static TEntity AddBaseMod<TEntity, TTarget, TSource>(this TEntity entity, Expression<Func<TEntity, TTarget>> targetExpr, Expression<Func<TEntity, TSource>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = BaseMod.Create(entity, targetExpr, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);

            return entity;
        }
    }
}
