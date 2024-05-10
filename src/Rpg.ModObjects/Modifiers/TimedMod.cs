using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class TimedMod : Mod
    {
        public TimedMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
        }

        public static Mod Create<TEntity, T1, T2>(int startTurn, int duration, TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<TEntity, T2>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Create<TimedMod, TEntity, T1, TEntity, T2>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            mod.Name = nameof(TimedMod);
            mod.Duration = ModDuration.Timed(startTurn, startTurn + duration - 1);

            return mod;
        }

        public static Mod Create<TEntity, T1>(int startTurn, int duration, TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Create<TimedMod, TEntity, T1, T1>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = nameof(TimedMod);
            mod.Duration = ModDuration.Timed(startTurn, startTurn + duration - 1);

            return mod;
        }

        public static Mod Create<TEntity, T1, T2>(string name, int startTurn, int duration, TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<TEntity, T2>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Create<TimedMod, TEntity, T1, TEntity, T2>(entity, targetExpr, entity, sourceExpr, diceCalcExpr);
            mod.Name = name;
            mod.Duration = ModDuration.Timed(startTurn, startTurn + duration - 1);

            return mod;
        }

        public static Mod Create<TEntity, T1, T2>(string name, int startTurn, int duration, TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Create<TimedMod, TEntity, T1, T2>(entity, targetExpr, value, diceCalcExpr);
            mod.Name = name;
            mod.Duration = ModDuration.Timed(startTurn, startTurn + duration);

            return mod;
        }
    }
}
