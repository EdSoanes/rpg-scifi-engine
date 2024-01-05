using System.Linq.Expressions;

namespace Rpg.Sys.Modifiers
{
    public class BaseModifier : Modifier
    {
        public BaseModifier() 
        {
            ModifierType = ModifierType.Base;
            ModifierAction = ModifierAction.Replace;
        }

        public override ModifierExpiry SetExpiry(int turn)
        {
            Expiry = ModifierExpiry.Active;
            return Expiry;
        }

        public static Modifier Create<TEntity, T1>(TEntity entity, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null) 
            where TEntity : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity, T1>(null, ModNames.Base, dice, null, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1>(TEntity entity, string name, Dice dice, Expression<Func<TEntity, T1>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity, T1>(null, name, dice, null, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, T2>(TEntity entity, Expression<Func<TEntity, T1>> sourceExpr, Expression<Func<TEntity, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity, T2>(entity, null, null, sourceExpr, entity, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity? entity, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity2, T2>(entity, null, null, sourceExpr, target, targetExpr, diceCalcExpr);

        public static Modifier Create<TEntity, T1, TEntity2, T2>(TEntity? entity, string? name, Dice? dice, Expression<Func<TEntity, T1>>? sourceExpr, TEntity2 target, Expression<Func<TEntity2, T2>> targetExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModdableObject
            where TEntity2 : ModdableObject
                => _Create<BaseModifier, TEntity, T1, TEntity2, T2>(entity, name, dice, sourceExpr, target, targetExpr, diceCalcExpr);

        public static Modifier CreateByPath(ModdableObject entity, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => _CreateByPath<BaseModifier>(entity, null, dice, targetPropPath, diceCalcExpr);

        public static Modifier CreateByPath<T1>(ModdableObject entity, string name, Dice dice, string targetPropPath, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            => _CreateByPath<BaseModifier>(entity, name, dice, targetPropPath, diceCalcExpr);
    }
}
