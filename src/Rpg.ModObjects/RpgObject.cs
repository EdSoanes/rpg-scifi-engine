using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : RpgLifecycleObject, INotifyPropertyChanged
    {
        [JsonProperty] public Dictionary<string, ModSet> ModSets { get; private init; }
        [JsonProperty] public Dictionary<string, Prop> Props {  get; private init; }
        [JsonProperty] public Dictionary<string, State> States { get; private init; }

        [JsonProperty] public string Id { get; private init; }
        [JsonProperty] public PropRef? ParentRef { get; private set; }

        [JsonProperty] public string Archetype { get; private init; }

        [JsonProperty] public string Name { get; protected set; }

        [JsonProperty] public string[] Archetypes { get; private init; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
        {
            Id = this.NewId();
            Archetype = this.GetType().Name;
            Name = this.GetType().Name;
            Archetypes = this.GetType().GetArchetypes();

            ModSets = new Dictionary<string, ModSet>();
            Props = new Dictionary<string, Prop>();
            States = new Dictionary<string, State>();
        }

        private void SetParent(RpgObject? parent)
        {
            if (parent == null)
                ParentRef = null;
            else
            {
                var path = parent.PathTo(this);
                ParentRef = new PropRef(parent.Id, string.Join('.', path));
                Name ??= path.LastOrDefault() ?? GetType().Name;
            }
        }

        protected T? SetAsChild<T>(T? newChild, T? oldChild)
            where T : RpgObject
        {
            if (oldChild?.ParentRef?.EntityId == Id)
                oldChild.SetParent(null);

            if (newChild != null)
            {
                newChild.SetParent(this);
                if (Graph != null && Graph.GetObject(newChild.Id) == null)
                    Graph.AddEntity(newChild);
            }

            return newChild;
        }

        private void SetAsChildren()
        {
            foreach (var propertyInfo in this.ScanForChildProperties())
            {
                var child = propertyInfo.GetValue(this) as RpgObject;
                child?.SetParent(this);
            }
        }

        #region ModSets

        public ModSet? GetModSet(string id)
            => ModSets.ContainsKey(id) ? ModSets[id] : null;

        public ModSet? GetModSetByName(string name)
            => ModSets.Values.FirstOrDefault(x => x.Name == name);

        public bool AddModSet(ModSet modSet)
        {
            if (!string.IsNullOrEmpty(modSet.OwnerId) && modSet.OwnerId != Id)
                throw new InvalidOperationException("ModSet.OwnerId is set but does not match the ModSetStore.EntityId");

            if (!ModSets.ContainsKey(modSet.Id))
            {
                modSet.OnCreating(Graph!, this);
                ModSets.Add(modSet.Id, modSet);

                if (Graph.Time.Now.Type != PointInTimeType.BeforeTime)
                {
                    modSet.OnTimeBegins();
                    modSet.OnStartLifecycle();
                }

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
                existing.SetExpired();
                Graph?.RemoveMods(existing.Mods.Where(x => x is Synced).ToArray());

                return ModSets.Remove(existing.Id);
            }

            return false;
        }

        private void OnStartModSetLifecycle()
        {
            foreach (var modSet in ModSets.Values)
            {
                if (!modSet.Mods.Any())
                    modSet.Mods.AddRange(Graph!.GetActiveMods(x => x.OwnerId == modSet.Id));

                modSet.OnStartLifecycle();
            }
        }

        private void OnUpdateModSetLifecycle()
        {
            var toRemove = new List<ModSet>();
            foreach (var modSet in ModSets.Values)
            {
               var expiry = modSet.OnUpdateLifecycle();
                if (expiry == LifecycleExpiry.Destroyed)
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
                        mod.OnCreating(Graph!);
                        mod.OnTimeBegins();
                        mod.Behavior.OnAdding(Graph, prop, mod);

                        if (Graph.Time.Now.Type != PointInTimeType.BeforeTime)
                            mod.OnStartLifecycle();

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
                        Graph!.OnPropUpdated(mod.TargetPropRef);
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
                        Graph!.OnPropUpdated(mod.TargetPropRef);
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
                        Graph!.OnPropUpdated(mod.TargetPropRef);
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
                        Graph!.OnPropUpdated(mod.TargetPropRef);
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

        private void OnUpdatePropsLifecycle()
        {
            var toRemove = new List<Mod>();
            foreach (var prop in Props.Values)
            {
                foreach (var mod in prop.Mods)
                {
                    var oldExpiry = mod.Expiry;
                    var expiry = mod.OnUpdateLifecycle();

                    if (expiry == LifecycleExpiry.Destroyed)
                        toRemove.Add(mod);

                    if (expiry != oldExpiry)
                        Graph!.OnPropUpdated(mod.TargetPropRef);
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

        public void SetStateOn(string state)
            => GetState(state)?.On();

        public void SetStateOff(string state)
            => GetState(state)?.Off();

        public ModSet CreateStateInstance(string state, SpanOfTime? spanOfTime = null)
            => GetState(state)!.CreateInstance(spanOfTime);

        private void OnStartStateLifecycle()
        {
            foreach (var state in States.Values)
                state.OnStartLifecycle();
        }

        public bool OnUpdateStateLifecycle()
        {
            var updated = false;
            foreach (var state in States.Values)
            {
                var oldExpiry = state.Expiry;
                var newExpiry = state.OnUpdateLifecycle();
                updated |= oldExpiry != newExpiry;
            }

            return updated;
        }

        #endregion States

        public bool IsA(string type) 
            => Archetypes.Contains(type);

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            SetAsChildren();

            base.OnCreating(graph, entity);

            ModSets.Clear();
            Props.Clear();
            States.Clear();

            foreach (var propInfo in RpgReflection.ScanForModdableProperties(this))
            {
                var val = this.PropertyValue(propInfo.Name, out var propExists);
                if (val != null)
                {
                    if (Graph == null)
                        throw new InvalidOperationException("Graph is null");
                    if (val is Dice dice)
                        AddMods(new Initial(Id, propInfo.Name, dice));
                    else if (val is int i)
                        AddMods(new Initial(Id, propInfo.Name, i));

                    var (min, max) = propInfo.GetPropertyThresholds();
                    if (min != null || max != null)
                        AddMods(new Threshold(Id, propInfo.Name, min ?? int.MinValue, max ?? int.MaxValue));
                }
            }

            var states = State.CreateOwnerStates(this);
            foreach (var state in states)
            {
                state.OnCreating(Graph);
                States.Add(state.Name, state);
            }
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            foreach (var state in States.Values)
                state.OnRestoring(Graph);

            foreach (var prop in Props.Values)
                foreach (var mod in prop.Mods)
                    mod.OnRestoring(graph);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            foreach (var mod in GetMods())
                mod.OnStartLifecycle();

            OnStartModSetLifecycle();
            OnStartStateLifecycle();

            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            foreach (var mod in GetMods())
                mod.OnUpdateLifecycle();

            OnUpdateModSetLifecycle();
            OnUpdatePropsLifecycle();

            return Expiry;
        }
    }
}
