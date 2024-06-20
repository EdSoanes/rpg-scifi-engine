using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods
{
    public abstract class Modification : ILifecycle
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; private set; }
        [JsonProperty] public string? OwnerArchetype { get; private set; }

        [JsonProperty] public string? InitiatorId { get; private set; }
        [JsonProperty] public string? InitiatorArchetype { get; private set; }

        [JsonProperty] public string? RecipientId { get; private set; }
        [JsonProperty] public string? RecipientArchetype { get; private set; }

        [JsonProperty] public string Name { get; set; }

        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }

        public LifecycleExpiry Expiry { get => Lifecycle.Expiry; protected set { } }

        [JsonConstructor] protected Modification() { }

        public Modification(ILifecycle lifecycle, string? name = null)
        {
            Id = this.NewId();
            Lifecycle = lifecycle;
            Name = name ?? GetType().Name;
        }

        public Modification(string name, ILifecycle lifecycle, params Mod[] mods)
            : this(lifecycle, name)
        {
            AddMods(mods);
        }

        public Modification AddOwner(RpgObject? owner)
        {
            OwnerId = owner?.Id;
            OwnerArchetype = owner?.GetType().Name;

            return this;
        }

        public Modification AddRecipient(RpgObject? recipient)
        {
            RecipientId = recipient?.Id;
            RecipientArchetype = recipient?.GetType().Name;

            return this;
        }

        public Modification AddInitiator(RpgObject? initiator)
        {
            InitiatorId = initiator?.Id;
            InitiatorArchetype = initiator?.GetType().Name;

            return this;
        }

        public Modification AddMods(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.SyncedToId = Id;
                Mods.Add(mod);
            }

            return this;
        }

        public Mod[] GetMods(string prop, Func<Mod, bool>? filterFunc = null)
            => Mods.Where(x => x.Prop == prop && (filterFunc?.Invoke(x) ?? true)).ToArray();

        //public Modification[] SubSets(RpgGraph graph)
        //{
        //    var subSets = new List<Modification>();
        //    foreach (var modGroup in Mods.GroupBy(x => $"{x.EntityId}.{x.Prop}"))
        //    {
        //        var mods = modGroup.ToArray();
        //        var dice = graph.CalculateModsValue(mods);

        //        var subSet = new Modification(modGroup.Key, Lifecycle, mods);
        //        subSets.Add(subSet);
        //    }

        //    return subSets.ToArray();
        //}

        public virtual void SetExpired(TimePoint currentTime)
            => Lifecycle.SetExpired(currentTime);

        public void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
            OwnerId ??= entity?.Id;

            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.SyncedToId == Id).ToList();

            Lifecycle.OnBeginningOfTime(graph, entity);
        }

        public LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnStartLifecycle(graph, currentTime);

        public LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null)
            => Lifecycle.OnUpdateLifecycle(graph, currentTime);
    }
}
