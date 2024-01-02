using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class TimedModifier : Modifier
    {
        public TimedModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            IsPermanent = false;
        }

        public static Modifier Create<TEntity, T1, T2>(int untilTurn, TEntity entity, string name, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => Create(untilTurn, entity, name, null, sourceExpr, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(int untilTurn, TEntity entity, string? name, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => Create<TEntity, T1, TEntity, T1>(untilTurn, entity, name, dice, null, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(int untilTurn, TEntity? entity, string? name, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => Create(untilTurn, entity, name, null, sourceExpr, target, targetExpr, diceCalcExpr);

        protected static Modifier Create<TEntity, T1, TEntity2, T2>(int untilTurn, TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
        {
            var mod = _Create<TimedModifier, TEntity, T1, TEntity2, T2>(entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);
            mod.RemoveOnTurn = untilTurn;

            return mod;
        }
    }
}
