using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public abstract class ActionGroup
    {
        [JsonProperty] public string Name { get; protected init; }
        [JsonProperty] public List<ActionGroupItem> Items { get; private set; } = new();

        public void Add(string ownerArchetype, string actionName, bool optional = true)
            => Add(new ActionGroupItem(ownerArchetype, actionName, optional));

        public void Add(ActionGroupItem item)
        {
            if (!Contains(item))
                Items.Add(item);
        }

        public void InsertAt(ActionGroupItem item, int idx)
        {
            Remove(item);
            if (idx >= Items.Count())
                Items.Add(item);
            else if (idx < 0)
                Items.Insert(0, item);
            else
                Items.Insert(idx, item);
        }

        public void InsertBefore(ActionGroupItem item, ActionGroupItem beforeItem)
        {
            Remove(item);
            InsertAt(item, IndexOf(beforeItem));
        }

        public void InsertAfter(ActionGroupItem item, ActionGroupItem afterItem)
        {
            Remove(item);
            InsertAt(item, IndexOf(afterItem) + 1);
        }

        public bool Contains(string ownerArchetype, string actionName)
            => Items.Any(x => x.ActionName == actionName && x.OwnerArchetype == ownerArchetype);

        public bool Contains(ActionGroupItem item)
            => Items.Any(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);

        public void Remove(ActionGroupItem item)
        {
            var res = Items.Find(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);
            if (res != null)
                Items.Remove(res);
        }

        public int IndexOf(ActionGroupItem item)
            => Items.FindIndex(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);
    }
}
