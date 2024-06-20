using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects.Actions
{
    public class RpgActionStore<TOwner> : ModBaseStore<string, ActionModification<TOwner>>
        where TOwner : RpgObject
    {
        public RpgActionStore(string entityId)
            : base(entityId) { }

        public void Add(params ActionModification<TOwner>[] actions)
        {
            foreach (var action in actions)
            {
                if (!Contains(action))
                {
                    Items.Add(action.Name, action);
                    if (Graph != null)
                    {
                        var entity = Graph.GetEntity(EntityId!);
                        action.OnBeginningOfTime(Graph!, entity!);
                    }
                }
            }
        }

        public override void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeginningOfTime(graph, entity);
            foreach (var cmd in Get())
                cmd.OnBeginningOfTime(graph, entity);
        }
    }
}
