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

        protected List<ModdableObject> Descendants(object obj)
        {
            var res = new List<ModdableObject>();
            var entity = obj as ModdableObject;
            if (entity != null)
                res.Add(entity);

            if (!obj.GetType().IsPrimitive)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                    foreach (var item in items)
                    {
                        var childEntities = Descendants(item);
                        res.AddRange(childEntities);
                    }
                }
            }

            return res;
        }
    }
}
