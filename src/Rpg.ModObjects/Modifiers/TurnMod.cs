using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class TurnMod : Mod
    {
        public TurnMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Accumulate;
        }

        public override void OnAdd(int turn)
        {
            Duration = ModDuration.OnNewTurn(turn);
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(TurnMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue>(string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = name;

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TSource, TSourceValue>(entity, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = nameof(TurnMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<TurnMod, TTarget, TTargetValue, TSource, TSourceValue>(entity, targetExpr, source, sourceExpr, diceCalcExpr);
            mod.Name = name;

            return mod;
        }
    }

    public static class TurnModExtensions
    {
        public static void AddTurnMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = TurnMod.Create(entity, targetExpr, value, diceCalcExpr);
            entity.AddMod(mod);
        }

        public static void AddTurnMod<TEntity, T1>(this TEntity entity, string name, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = TurnMod.Create(name, entity, targetExpr, value, diceCalcExpr);
            entity.AddMod(mod);
        }
    }
}