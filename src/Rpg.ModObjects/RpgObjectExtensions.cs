using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private class PropertyRef
        {
            public RpgObject? Entity { get; set; }
            public string? Prop { get; set; }
        }

        public static void Merge(this List<PropRef> target, PropRef propRef)
        {
            if (!target.Any(x => x == propRef))
                target.Add(propRef);
        }

        public static void Merge(this List<PropRef> target, IEnumerable<PropRef> source)
        {
            foreach (var a in source)
                target.Merge(a);
        }

        public static void TriggerUpdate<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr)
            where TTarget : RpgObject
        {
            var propRef = PropRef.CreatePropRef(entity, targetExpr);
            entity.TriggerUpdate(propRef);
        }

        public static ModObjectPropDescription Describe<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : RpgObject
        {
            var propRef = PropRef.CreatePropRef(entity, targetExpr);
            return entity.Describe(propRef.Prop);
        }

        public static ModSet? AddModSet<T>(this T entity, string name, ModBehavior behavior, params Mod[] mods)
            where T : RpgObject
        {
            var modSet = new ModSet(entity.Id, name, behavior, mods);
            return entity.AddModSet(modSet)
                ? modSet
                : null;
        }


        public static T AddModSet<T>(this T entity, string name, Action<ModSet> addAction)
            where T : RpgObject
        {
            var modSet = new ModSet(entity.Id, name);
            addAction.Invoke(modSet);
            entity.AddModSet(modSet);

            return entity;
        }

        public static T AddModSet<T>(this T entity, string name, ModBehavior behavior, Action<ModSet> addAction)
            where T : RpgObject
        {
            var modSet = new ModSet(entity.Id, name, behavior);
            addAction.Invoke(modSet);
            entity.AddModSet(modSet);

            return entity;
        }

    }
}
