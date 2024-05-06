using System.Linq.Expressions;
using Rpg.Sys.Moddable;

namespace Rpg.Sys.Modifiers
{
    public class TimedModifier : Modifier
    {
        public TimedModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
        }

        public static Modifier Create<TEntity, T1, T2>(int startTurn, int duration, TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
                => Create(startTurn, duration, entity, null, null, sourceExpr, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(int startTurn, int duration, TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
                => Create<TEntity, T1, TEntity, T1>(startTurn, duration, entity, null, dice, null, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(int startTurn, int duration, TEntity? entity, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
            where TEntity2 : ModObject
                => Create(startTurn, duration, entity, null, null, sourceExpr, target, targetExpr, diceCalcExpr);

        protected static Modifier Create<TEntity, T1, TEntity2, T2>(int startTurn, int duration, TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
            where TEntity2 : ModObject
        {
            var mod = _Create<TimedModifier, TEntity, T1, TEntity2, T2>(startTurn, duration, entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);
            return mod;
        }
    }
}
