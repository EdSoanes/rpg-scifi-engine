using Newtonsoft.Json;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods
{
    public abstract class Modification
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? InitiatorObjId { get; private set; }
        [JsonProperty] public string? RecipientObjId { get; private set; }
        [JsonProperty] public string? OwnerObjId { get; private set; }

        [JsonProperty] public string Name { get; set; }

        [JsonIgnore] public List<Mod> Mods { get; private set; } = new List<Mod>();
        [JsonProperty] public ILifecycle Lifecycle { get; protected set; }
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
            OwnerObjId = owner?.Id;
            return this;
        }

        public Modification AddRecipient(RpgObject? recipient)
        {
            RecipientObjId = recipient?.Id;
            return this;
        }

        public Modification AddInitiator(RpgObject? initiator)
        {
            InitiatorObjId = initiator?.Id;
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

        public void OnGraphCreating(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
            if (!Mods.Any())
                Mods = graph.GetMods().Where(x => x.SyncedToId == Id).ToList();
        }

        public void OnObjectsCreating() { }

        public void OnAdding(RpgGraph graph, TimePoint currentTime)
            => Lifecycle.StartLifecycle(graph, currentTime);

        public void OnUpdating(RpgGraph graph, TimePoint currentTime)
            => Lifecycle.UpdateLifecycle(graph, currentTime);
    }

    public abstract class Modification<T> : Modification
        where T : RpgObject
    {
        public Modification AddLifecycle()
        {
            return this;
        }
    }
}
