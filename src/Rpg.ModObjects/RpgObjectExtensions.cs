using NanoidDotNet;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzöåäABCDEFGHIJKLMNOPQRSTUVWXYZÖÅÄ_-";
        private const int Size = 12;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";

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

        public static ModObjectPropDescription Describe<TEntity, T1>(this TEntity entity, Expression<Func<TEntity, T1>> targetExpr)
            where TEntity : RpgObject
        {
            var propRef = PropRef.CreatePropRef(entity, targetExpr);
            return entity.Describe(propRef.Prop);
        }

        public static T AddModSet<T>(this T entity, string name, Action<ModSet> addAction)
            where T : RpgObject
                => AddModSet<T>(entity, name, new PermanentLifecycle(), addAction);

        public static T AddModSet<T>(this T entity, string name, ILifecycle lifecycle, Action<ModSet> addAction)
            where T : RpgObject
        {
            var modSet = new ModSet(lifecycle, name)
                .AddOwner(entity);

            addAction.Invoke(modSet);
            entity.AddModSet(modSet);

            return entity;
        }
    }
}
