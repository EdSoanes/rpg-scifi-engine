using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects.Actions
{
    public class RpgActionStore : ModBaseStore<string, Action>
    {
        public RpgActionStore(string entityId)
            : base(entityId) { }

        public void Add(params Action[] actions)
        {
            foreach (var action in actions)
            {
                if (!Contains(action))
                    Items.Add(action.Name, action);
            }
        }
    }
}
