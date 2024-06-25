using NanoidDotNet;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        private const int Size = 12;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";

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

        internal static object? PropertyValue(this object? entity, string path, out bool propExists)
        {
            var propInfo = RpgReflection.ScanForProperty(entity, path, out var pathEntity);

            propExists = pathEntity != null;
            var val = propInfo?.GetValue(pathEntity, null);

            return val;
        }

        internal static T? PropertyValue<T>(this RpgObject? entity, string path)
        {
            var propInfo = RpgReflection.ScanForProperty(entity, path, out var pathEntity);
            var res = propInfo?.GetValue(pathEntity, null);

            if (res is T)
                return (T?)res;

            if (typeof(T) == typeof(string))
                return (T?)(object?)res?.ToString();

            return default;
        }

        internal static void PropertyValue(this object? entity, string path, object? value)
        {
            var propInfo = RpgReflection.ScanForModdableProperty(entity, path, out var pathEntity);
            if (propInfo?.SetMethod != null)
            {
                if (propInfo.PropertyType == typeof(int) && value is Dice)
                    propInfo?.SetValue(pathEntity, ((Dice)value).Roll());
                else
                    propInfo?.SetValue(pathEntity, (Dice)value);
            }
        }
    }
}
