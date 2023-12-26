using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class HealingModifier : Modifier
    {
        public HealingModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            RemoveOnTurn = RemoveTurn.WhenZero;
            IsPermanent = false;
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : Entity
                => _Create<HealingModifier, TEntity, T1, TEntity, T1>(null, ModNames.Damage, dice, null, target, targetExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr)
            where TEntity : Entity
            where TEntity2 : Entity
                => _Create<HealingModifier, TEntity, T1, TEntity2, T2>(entity, ModNames.Damage, null, sourceExpr, target, targetExpr);
    }
}
