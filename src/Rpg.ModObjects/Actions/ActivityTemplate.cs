using Newtonsoft.Json;

namespace Rpg.ModObjects.Actions
{
    public abstract class ActivityTemplate
    {
        [JsonProperty] public string Name { get; protected init; }
        [JsonProperty] public List<ActivityTemplateItem> Items { get; private set; } = new();

        public void Add(string ownerArchetype, string actionName, bool optional = true)
            => Add(new ActivityTemplateItem(ownerArchetype, actionName, optional));

        public void Add(ActivityTemplateItem item)
        {
            if (!Contains(item))
                Items.Add(item);
        }

        public void InsertAt(ActivityTemplateItem item, int idx)
        {
            Remove(item);
            if (idx >= Items.Count())
                Items.Add(item);
            else if (idx < 0)
                Items.Insert(0, item);
            else
                Items.Insert(idx, item);
        }

        public void InsertBefore(ActivityTemplateItem item, ActivityTemplateItem beforeItem)
        {
            Remove(item);
            InsertAt(item, IndexOf(beforeItem));
        }

        public void InsertAfter(ActivityTemplateItem item, ActivityTemplateItem afterItem)
        {
            Remove(item);
            InsertAt(item, IndexOf(afterItem) + 1);
        }

        public bool Contains(string ownerArchetype, string actionName)
            => Items.Any(x => x.ActionName == actionName && x.OwnerArchetype == ownerArchetype);

        public bool Contains(ActivityTemplateItem item)
            => Items.Any(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);

        public void Remove(ActivityTemplateItem item)
        {
            var res = Items.Find(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);
            if (res != null)
                Items.Remove(res);
        }

        public int IndexOf(ActivityTemplateItem item)
            => Items.FindIndex(x => x.ActionName == item.ActionName && x.OwnerArchetype == item.OwnerArchetype);
    }
}
