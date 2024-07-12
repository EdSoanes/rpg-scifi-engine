using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : INotifyPropertyChanged, ILifecycle
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public Dictionary<string, ModSet> ModSets { get; private init; }
        [JsonProperty] public Dictionary<string, Prop> Props {  get; private init; }

        [JsonProperty] public string Id { get; private init; }

        [JsonProperty] public string Archetype { get; private init; }

        [JsonProperty] public string Name { get; protected init; }

        [JsonProperty] public string[] Archetypes { get; private init; }

        [JsonProperty] public LifecycleExpiry Expiry { get; protected set; } = LifecycleExpiry.Pending;

        //[JsonProperty] internal PropStore PropStore { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
        {
            Id = this.NewId();
            Archetype = GetType().Name;
            Name = GetType().Name;
            Archetypes = this.GetType().GetArchetypes();

            ModSets = new Dictionary<string, ModSet>();
            Props = new Dictionary<string, Prop>();

            //PropStore = new PropStore(Id);
        }

        #region ModSets

        public ModSet? GetModSet(string id)
            => ModSets.ContainsKey(id) ? ModSets[id] : null;

        public bool AddModSet(ModSet modSet)
        {
            if (!string.IsNullOrEmpty(modSet.OwnerId) && modSet.OwnerId != Id)
                throw new InvalidOperationException("ModSet.OwnerId is set but does not match the ModSetStore.EntityId");

            if (!ModSets.ContainsKey(modSet.Id))
            {
                modSet.OnAdding(this);
                ModSets.Add(modSet.Id, modSet);

                Graph!.AddMods(modSet.Mods.ToArray());
                modSet.Lifecycle.OnStartLifecycle(Graph!, Graph.Time.Current);

                return true;
            }

            return false;
        }

        public bool AddModSets(params ModSet[] modSets)
        {
            foreach (var modSet in modSets)
                AddModSet(modSet);

            return true;
        }

        public bool RemoveModSet(string modSetId)
        {
            var existing = GetModSet(modSetId);
            if (existing != null)
            {
                existing.Lifecycle.SetExpired(Graph!.Time.Current);
                Graph?.RemoveMods(existing.Mods.Where(x => x.Lifecycle is SyncedLifecycle).ToArray());

                return ModSets.Remove(existing.Id);
            }

            return false;
        }

        private void OnStartModSetLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            foreach (var modSet in ModSets.Values)
            {
                if (!modSet.Mods.Any())
                    modSet.Mods.AddRange(graph.GetActiveMods(x => x.OwnerId == modSet.Id));

                modSet.Lifecycle.OnStartLifecycle(graph, currentTime);
            }
        }

        private void OnUpdateModSetLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            var toRemove = new List<ModSet>();
            foreach (var modSet in ModSets.Values)
            {
                modSet.Lifecycle.OnUpdateLifecycle(graph, currentTime);
                if (modSet.Lifecycle.Expiry == LifecycleExpiry.Remove)
                    toRemove.Add(modSet);
            }

            foreach (var remove in toRemove)
                RemoveModSet(remove.Id);
        }

        #endregion ModSets

        #region Mods

        public Prop? GetProp(string? prop, bool create = false)
        {
            if (string.IsNullOrEmpty(prop))
                return null;

            if (Props.ContainsKey(prop))
                return Props[prop];

            if (create)
            {
                var created = new Prop(Id, prop);
                Props.Add(prop, created);

                return created;
            }

            return null;
        }

        public Prop[] GetProps()
            => Props.Values.ToArray();

        public Mod? GetMod(string id)
            => Props.Values
                .SelectMany(x => x.Mods)
                .FirstOrDefault(x => x.Id == id);

        public Mod[] GetMods()
            => Props.Values
                .SelectMany(x => x.Mods)
                .ToArray();

        public Mod[] GetMods(string? prop, Func<Mod, bool>? filterFunc = null)
            => GetProp(prop)?.Mods
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray() ?? Array.Empty<Mod>();

        public Mod[] GetActiveMods()
            => Props.Values
                .SelectMany(x => x.GetActive())
                .ToArray();

        public Mod[] GetActiveMods(string? prop, Func<Mod, bool>? filterFunc = null)
            => GetProp(prop)?.GetActive()
                .Where(x => filterFunc == null || filterFunc(x))
                .ToArray() ?? Array.Empty<Mod>();

        public void AddMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    foreach (var mod in modGroup)
                    {
                        var prop = GetProp(mod.Prop, create: true)!;
                        prop.Remove(mod);
                        mod.OnAdding(Graph!, prop, Graph!.Time.Current);

                        Graph!.OnPropUpdated(prop);
                    }
                }
                else
                    Graph!.AddMods([.. modGroup]);
            }
        }

        public void RemoveMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    var toRemove = new List<Prop>();
                    foreach (var mod in modGroup)
                    {
                        var prop = GetProp(mod.Prop);
                        if (prop != null)
                        {
                            mod.OnRemoving(Graph!, prop);
                            prop.Remove(mod);
                            Graph!.OnPropUpdated(prop);

                            if (!prop.Mods.Any())
                                toRemove.Add(prop);
                        }
                    }

                    foreach (var prop in toRemove)
                        Props.Remove(prop.Prop);
                }
                else
                    Graph!.RemoveMods([.. modGroup]);
            }
        }

        private void OnUpdatePropsLifecycle(TimePoint currentTime)
        {
            var toRemove = new List<Mod>();
            foreach (var prop in Props.Values)
            {
                foreach (var mod in prop.Mods)
                {
                    var oldExpiry = mod.Expiry;
                    mod.OnUpdating(Graph!, prop, currentTime);
                    var expiry = mod.Expiry;

                    if (expiry == LifecycleExpiry.Remove)
                        toRemove.Add(mod);

                    if (expiry != oldExpiry)
                        Graph!.OnPropUpdated(mod);
                }
            }

            RemoveMods(toRemove.ToArray());
        }

        #endregion

        public bool IsA(string type) 
            => Archetypes.Contains(type);

        internal ModObjectPropDescription Describe(string prop)
            => new ModObjectPropDescription(Graph!, this, prop);

        protected virtual void OnLifecycleStarting() { }
        public virtual void OnUpdating(RpgGraph graph, TimePoint time) { }

        public void SetExpired(TimePoint currentTime)
        {
        }

        public virtual void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            Graph = graph;
        }

        public virtual void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null)
        {
            foreach (var propInfo in RpgReflection.ScanForModdableProperties(this))
            {
                var val = this.PropertyValue(propInfo.Name, out var propExists);
                if (val != null)
                {
                    if (val is Dice dice)
                        this.InitMod(propInfo.Name, dice);
                    else if (val is int i)
                        this.InitMod(propInfo.Name, i);

                    var (min, max) = propInfo.GetPropertyThresholds();
                    if (min != null || max != null)
                        this.ThresholdMod(propInfo.Name, min ?? int.MinValue, max ?? int.MaxValue);
                }
            }

            OnLifecycleStarting();

            Expiry = LifecycleExpiry.Active;
        }

        public virtual LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            OnStartModSetLifecycle(graph, currentTime);
            return Expiry;
        }

        public virtual LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            OnUpdateModSetLifecycle(graph, currentTime);
            OnUpdatePropsLifecycle(currentTime);

            return Expiry;
        }
    }
}
