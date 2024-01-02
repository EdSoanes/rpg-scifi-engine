using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class DamageModifier : Modifier
    {
        public DamageModifier()
        {
            ModifierType = ModifierType.Transient;
            ModifierAction = ModifierAction.Sum;
            RemoveOnTurn = RemoveTurn.WhenZero;
            IsPermanent = false;
        }

        public static Modifier Create<TEntity, T1>(Dice dice, TEntity target, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : ModdableObject
                => _Create<DamageModifier, TEntity, T1, TEntity, T1>(null, ModNames.Damage, dice, null, target, targetExpr, () => Rules.Minus);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<DamageModifier, TEntity, T1, TEntity2, T2>(entity, ModNames.Damage, null, sourceExpr, target, targetExpr, () => Rules.Minus);
    }
}
