using Rpg.Sys.Components;

namespace Rpg.Sys.GraphOperations
{
    public abstract class Operation
    {
        protected readonly Graph Graph;
        protected readonly ModStore ModStore;
        protected readonly EntityStore EntityStore;
        protected readonly List<Condition> ConditionStore;

        public Operation(Graph graph, ModStore modStore, EntityStore entityStore, List<Condition> conditionStore)
        {
            Graph = graph;
            ModStore = modStore;
            EntityStore = entityStore;
            ConditionStore = conditionStore;
        }

        public bool Restoring { get; set; }
    }
}
