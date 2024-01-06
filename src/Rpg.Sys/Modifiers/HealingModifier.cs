using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class HealingModifier : Modifier
    {
        public HealingModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            EndTurn = RemoveTurn.WhenZero;
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : ModdableObject
                => _Create<HealingModifier, TEntity, T1, TEntity, T1>(null, ModNames.Damage, dice, null, target, targetExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<HealingModifier, TEntity, T1, TEntity2, T2>(entity, ModNames.Damage, null, sourceExpr, target, targetExpr);
    }
}
