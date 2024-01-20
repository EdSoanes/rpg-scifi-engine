using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.GraphOperations
{
    public class GetOp : Operation
    {
        public GetOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public T? Entity<T>(Guid? entityId)
            where T : ModdableObject
            => EntityStore.Get(entityId) as T;

        public Condition? Condition(string conditionName)
            => ConditionStore.FirstOrDefault(x => x.Name == conditionName);

        public Modifier[] Mods(Guid entityId)
            => ModStore.Get(entityId)
                ?.SelectMany(x => x.AllModifiers)
                ?.ToArray()
                ?? new Modifier[0];

        public Modifier[] Mods(Guid entityId, string prop)
            => ModStore.Get(entityId, prop)
                ?.AllModifiers ?? new Modifier[0];

        public Modifier[] Mods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => ModStore.Get(PropRef.FromPath(entity, expression, true))?.AllModifiers ?? new Modifier[0];

        public ModProp? ModProp<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
                => ModProp(PropRef.FromPath(entity, expression, true));

        public ModProp? ModProp(PropRef propRef)
            => ModStore.Get(propRef);

        public ModProp? ModProp(Guid entityId, string prop)
            => ModStore.Get(entityId, prop);

        public ModProp? ModProp(Guid modPropId)
            => ModStore.Values.FirstOrDefault(x => x.Id == modPropId);

        public ModProp[] AffectedModProps(ModProp modProp)
            => ModStore.GetAffectedModProps(modProp).ToArray();
    }
}
