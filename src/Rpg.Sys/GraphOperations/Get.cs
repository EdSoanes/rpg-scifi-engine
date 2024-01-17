using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class Get : Operation
    {
        public Get(Graph graph) 
            : base(graph) { }

        public T? Entity<T>(Guid entityId)
            where T : ModdableObject
            => Graph.Entities.Get(entityId) as T;

        public Condition? Condition(string conditionName)
            => Graph.Conditions.FirstOrDefault(x => x.Name == conditionName);

        public Modifier[] Mods(Guid entityId)
            => Graph.Mods.Get(entityId)
                ?.SelectMany(x => x.AllModifiers)
                ?.ToArray()
                ?? new Modifier[0];

        public Modifier[] Mods(Guid entityId, string prop)
            => Graph.Mods.Get(entityId, prop)
                ?.AllModifiers ?? new Modifier[0];
    }
}
