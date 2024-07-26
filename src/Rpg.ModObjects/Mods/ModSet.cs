using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public class ModSet : ModSetBase
    {
        [JsonConstructor] protected ModSet() { }

        public ModSet(string ownerId, ILifecycle lifecycle, string name)
            : base(ownerId, lifecycle, name) { }
    }

    public static class ModSetExtensions
    {
        public static T Add<T, TEntity>(this T modSet, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), entity, targetProp, dice, valueCalc);

        public static T Add<T, TEntity>(this T modSet, ModTemplate template, TEntity entity, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, dice, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), entity, targetExpr, sourceExpr, valueCalc);

        public static T Add<T, TEntity, TTargetValue, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TEntity, TSourceValue>(this T modSet, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), entity, targetProp, sourceExpr, valueCalc);

        public static T Add<T, TEntity, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, string targetProp, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), target, targetProp, source, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TSource, TSourceValue>(this T modSet, ModTemplate template, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetProp, source, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TEntity, TTargetValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), entity, targetExpr, dice, valueCalc);

        public static T Add<T, TEntity, TTargetValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, dice, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), entity, targetExpr, sourceExpr, valueCalc);

        public static T Add<T, TEntity, TTarget, TTargetValue, TSourceValue>(this T modSet, ModTemplate template, TEntity entity, Expression<Func<TEntity, TTargetValue>> targetExpr, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TEntity : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }

        public static T Add<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
                => modSet.Add(new SyncedMod(modSet.Id), target, targetExpr, source, sourceExpr, valueCalc);

        public static T Add<T, TTarget, TTargetValue, TSource, TSourceValue>(this T modSet, ModTemplate template, TTarget target, Expression<Func<TTarget, TTargetValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where T : ModSet
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = template
                .SetProps(target, targetExpr, source, sourceExpr, valueCalc)
                .Create();

            modSet.AddMods(mod);

            return modSet;
        }
    }
}
