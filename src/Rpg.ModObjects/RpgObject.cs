using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : INotifyPropertyChanged, ILifecycle
    {
        protected RpgGraph? Graph { get; private set; }

        [JsonProperty] public Dictionary<string, ModSet> ModSets { get; private init; }
        [JsonProperty] public Dictionary<string, Prop> Props {  get; private init; }
        [JsonProperty] public Dictionary<string, State> States { get; private init; }

        [JsonProperty] public string Id { get; private init; }

        [JsonProperty] public string Archetype { get; private init; }

        [JsonProperty] public string Name { get; protected set; }

        [JsonProperty] public string[] Archetypes { get; private init; }

        [JsonProperty] public LifecycleExpiry Expiry { get; protected set; } = LifecycleExpiry.Pending;

        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
        {
            Id = this.NewId();
            Archetype = GetType().Name;
            Name = GetType().Name;
            Archetypes = this.GetType().GetArchetypes();

            ModSets = new Dictionary<string, ModSet>();
            Props = new Dictionary<string, Prop>();
            States = new Dictionary<string, State>();
        }

        #region ModSets

        public ModSetBase? GetModSet(string id)
            => ModSets.ContainsKey(id) ? ModSets[id] : null;

        public ModSetBase? GetModSetByName(string name)
            => ModSets.Values.FirstOrDefault(x => x.Name == name);

        public bool AddModSet(ModSet modSet)
        {
            if (!string.IsNullOrEmpty(modSet.OwnerId) && modSet.OwnerId != Id)
                throw new InvalidOperationException("ModSet.OwnerId is set but does not match the ModSetStore.EntityId");

            if (!ModSets.ContainsKey(modSet.Id))
            {
                ModSets.Add(modSet.Id, modSet);
                modSet.OnBeforeTime(Graph!, this);

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

        #region Props

        public PropDesc? Describe(string propPath)
        {
            var exists = false;
            var (entity, prop) = this.FromPath(propPath);
            if (entity == null || prop == null)
                return null;

            var propDesc = new PropDesc
            {
                RootEntityId = Id,
                RootEntityArchetype = Archetype,
                RootEntityName = Name,
                RootProp = propPath,

                EntityId = entity!.Id,
                EntityArchetype = entity.Archetype,
                EntityName = entity.Name,
                Prop = prop,

                Value = Graph!.CalculatePropValue(entity, prop) ?? Dice.Zero,
                BaseValue = Graph!.CalculateBasePropValue(entity, prop) ?? Dice.Zero
            };

            propDesc.Mods = entity.GetActiveMods(prop)
                .Select(x => x.Describe(Graph!))
                .Where(x => x != null)
                .Cast<ModDesc>()
                .ToList();

            return propDesc;
        }

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

        #endregion Props

        #region Mods

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

        public void EnableMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    foreach (var mod in modGroup.Where(x => x.IsDisabled && Props.ContainsKey(x.Prop)))
                    {
                        mod.Enable();
                        Graph!.OnPropUpdated(mod);
                    }
                }
                else
                    Graph!.EnableMods([.. modGroup]);
            }
        }

        public void DisableMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    foreach (var mod in modGroup.Where(x => !x.IsDisabled && Props.ContainsKey(x.Prop)))
                    {
                        mod.Disable();
                        Graph!.OnPropUpdated(mod);
                    }
                }
                else
                    Graph!.DisableMods([.. modGroup]);
            }
        }

        public void ApplyMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    foreach (var mod in modGroup.Where(x => !x.IsApplied && Props.ContainsKey(x.Prop)))
                    {
                        mod.Apply();
                        Graph!.OnPropUpdated(mod);
                    }
                }
                else
                    Graph!.EnableMods([.. modGroup]);
            }
        }

        public void UnapplyMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
            {
                if (modGroup.Key == Id)
                {
                    foreach (var mod in modGroup.Where(x => x.IsApplied && Props.ContainsKey(x.Prop)))
                    {
                        mod.Unapply();
                        Graph!.OnPropUpdated(mod);
                    }
                }
                else
                    Graph!.DisableMods([.. modGroup]);
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

        #region States

        public State? GetState(string state)
            => States.ContainsKey(state) ? States[state] : null;

        public State? GetStateById(string id)
            => States.Values.FirstOrDefault(x => x.Id == id);

        public bool IsStateOn(string state)
            => GetState(state)?.IsOn ?? false;

        public bool SetStateOn(string state)
            => GetState(state)?.On() ?? false;

        public bool SetStateOff(string state)
            => GetState(state)?.Off() ?? false;

        public ModSet[] GetActiveConditionalStateInstances(string state)
            => Graph!.GetModSets(this, (x) => x.Name == state && x.Lifecycle is SyncedLifecycle && x.Expiry == LifecycleExpiry.Active);

        public ModSet[] GetActiveManualStateInstances(string state)
            => Graph!.GetModSets(this, (x) => x.Name == state && !(x.Lifecycle is SyncedLifecycle) && x.Expiry == LifecycleExpiry.Active);

        public ModSet CreateStateInstance(string state, ILifecycle? lifecycle = null)
            => GetState(state)!.CreateInstance(lifecycle ?? new TurnLifecycle());

        private void OnStartStateLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            foreach (var state in States.Values)
            {
                var stateExpiry = state.Lifecycle.OnStartLifecycle(graph, currentTime);
                if (stateExpiry == LifecycleExpiry.Active)
                {
                    var stateSets = GetActiveConditionalStateInstances(state.Name);
                    if (!stateSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        AddModSet(stateModSet);
                    }
                }
            }
        }

        public bool OnUpdateStateLifecycle(RpgGraph graph, TimePoint time)
        {
            var updated = false;

            foreach (var state in States.Values)
            {
                var stateExpiry = state.Lifecycle.OnUpdateLifecycle(graph, time);
                var activeConditionalSets = GetActiveConditionalStateInstances(state.Name);

                if (stateExpiry == LifecycleExpiry.Active)
                {
                    if (!activeConditionalSets.Any())
                    {
                        var stateModSet = state.CreateInstance();
                        AddModSet(stateModSet);
                        updated = true;
                    }
                }
                else
                {
                    foreach (var modSet in activeConditionalSets)
                    {
                        graph.RemoveModSet(modSet.Id);
                        updated = true;
                    }
                }
            }

            return updated;
        }

        #endregion States

        public bool IsA(string type) 
            => Archetypes.Contains(type);

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

            var states = State.CreateOwnerStates(this);
            foreach (var state in states)
            {
                state.OnAdding(graph);
                States.Add(state.Name, state);
            }

            Expiry = LifecycleExpiry.Active;
        }

        public virtual LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime)
        {
            OnStartModSetLifecycle(graph, currentTime);
            OnStartStateLifecycle(graph, currentTime);

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
