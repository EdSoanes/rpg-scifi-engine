using NanoidDotNet;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
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

        public static T AddModSet<T>(this T entity, string name, System.Action<ModSet> addAction)
            where T : RpgObject
                => AddModSet(entity, name, new PermanentLifecycle(), addAction);

        public static T AddModSet<T>(this T entity, string name, ILifecycle lifecycle, System.Action<ModSet> addAction)
            where T : RpgObject
        {
            var modSet = new ModSet(lifecycle, name)
                .AddOwner(entity);

            addAction.Invoke(modSet);
            entity.AddModSet(modSet);

            return entity;
        }

        public static T InitActionsAndStates<T>(this T entity, RpgGraph graph)
            where T : RpgEntity
        {
            var actions = entity.CreateActions();
            entity.ActionStore.Add(actions);
            entity.ActionStore.OnBeginningOfTime(graph, entity);

            var states = entity.CreateStates()
                .Union(entity.CreateStateActions(actions))
                .ToArray();

            foreach (var state in states)
            {
                state.OnBeforeTime(graph, entity);
                state.OnBeginningOfTime(graph, entity);
            }

            entity.StateStore.Add(states);
            entity.StateStore.OnBeginningOfTime(graph, entity);

            return entity;
        }
    }
}
