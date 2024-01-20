using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.GraphOperations
{
    public class CountOp : Operation
    {
        public CountOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public int Entities()
            => EntityStore.Count;

        public int Condition()
            => ConditionStore.Count;

        public int Mods()
            => ModStore.Count;
    }
}
