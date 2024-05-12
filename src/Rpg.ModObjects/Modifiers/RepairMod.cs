using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    public class RepairMod : Mod
    {
        public RepairMod()
        {
            ModifierType = ModType.Transient;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.OnValueZero();
        }

        public static Mod Create<TTarget, TTargetValue>(TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice value)
            where TTarget : ModObject
        {
            var mod = Create<RepairMod, TTarget, TTargetValue, TTargetValue>(entity, targetExpr, value);
            mod.Name = nameof(DamageMod);

            return mod;
        }

        public static Mod Create<TTarget, TTargetValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Create<RepairMod, TTarget, TTargetValue, TSource, TSourceValue>(target, targetExpr, source, sourceExpr);
            mod.Name = nameof(DamageMod);

            return mod;
        }
    }

    public static class RepairModExtensions
    {
        public static void AddRepairMod<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr, Dice dice)
            where TEntity : ModObject
        {
            var mod = RepairMod.Create(entity, targetExpr, dice);
            entity.AddMod(mod);
        }

        public static void AddRepairMod<TTarget, TTargetValue, TSource, TSourceValue>(this TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = RepairMod.Create(target, targetExpr, source, sourceExpr);
            target.AddMod(mod);
        }
    }
}
