using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class TransientModifier : Modifier
    {
        public TransientModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            RemoveOnTurn = RemoveTurn.WhenZero;
            IsPermanent = false;
        }

        public static Modifier Create<TEntity, T1, T2>(TEntity entity, string name, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : Entity
                => Modifier._Create<TransientModifier, TEntity, T1, TEntity, T2>(entity, name, null, sourceExpr, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(TEntity entity, string? name, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : Entity
                => Modifier._Create<TransientModifier, TEntity, T1, TEntity, T1>(entity, name, dice, null, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : Entity
            where TEntity2 : Entity
                => Modifier._Create<TransientModifier, TEntity, T1, TEntity2, T2>(entity, name, null, sourceExpr, target, targetExpr, diceCalcExpr);
    }
}
