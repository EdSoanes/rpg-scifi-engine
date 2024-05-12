using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Rpg.ModObjects.Modifiers
{
    public class PermanentMod : Mod
    {
        public PermanentMod() 
        {
            ModifierType = ModType.Base;
            ModifierAction = ModAction.Accumulate;
            Duration = ModDuration.Permanent();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null) 
            where TTarget : ModObject
        {
            var mod = Create<PermanentMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(PermanentMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<PermanentMod, TTarget, TTargetValue, TTarget, TSourceValue>(target, targetExpr, target, sourceExpr, diceCalcExpr);
            mod.Name = nameof(PermanentMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<PermanentMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = nameof(PermanentMod);

            return mod;
        }
    }

    public static class PermanentModExtensions
    {
        public static Mod AddPermanentMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = PermanentMod.Create(entity, targetExpr, dice, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }

        public static Mod AddPermanentMod<TEntity, TTarget, TSource>(this TEntity entity, Expression<Func<TEntity, TTarget>> targetExpr, Expression<Func<TEntity, TSource>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = PermanentMod.Create(entity, targetExpr, sourceExpr, diceCalcExpr);
            entity.AddMod(mod);

            return mod;
        }
    }
}
