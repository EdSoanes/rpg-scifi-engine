using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
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
        [JsonProperty] public ModSetDictionary ModSets { get; set; }
        [JsonProperty] public PropsDictionary Props { get; set; }
        [JsonProperty] public StatesDictionary States { get; set; }

        [JsonProperty] public string Id { get; private set; }
        [JsonProperty] public string? OwnerId { get; set; }
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string[] Archetypes { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RpgObject()
            : base()
        {
            Id = this.NewId();
            Archetype = this.GetType().Name;
            Name = this.GetType().Name;
            Archetypes = this.GetType().GetArchetypes();

            ModSets = new ModSetDictionary();
            Props = new PropsDictionary();
            States = new StatesDictionary();
        }

        public RpgObject(string ownerId)
            : this()
        {
            OwnerId = ownerId;
        }

        public RpgObject(string ownerId, Lifespan lifespan)
            : this(ownerId)
        {
            Lifespan = lifespan;
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
                if (Graph != null)
                {
                    modSet.OnCreating(Graph, this);
                    if (Graph.Time.Now.Type != PointInTimeType.BeforeTime)
                    {
                        modSet.OnTimeBegins();
                        modSet.OnStartLifecycle();
                    }
                }
                
                ModSets.Add(modSet.Id, modSet);
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

            if (Graph != null)
            {
                var existing = GetModSet(modSetId);
                if (existing != null)
                {
                    existing.SetExpired();
                    var syncedMods = ModFilters.SyncedToOwner(existing.Mods, existing.Id);
                    Graph.RemoveMods([.. syncedMods]);

                    return ModSets.Remove(existing.Id);
                }
            }

            return false;
        }

        #endregion ModSets

        #region Props

        public PropDescription? Describe(string propPath)
        {
            var (entity, prop) = this.FromPath(propPath);
            if (entity == null || prop == null)
                return null;

            var propDesc = new PropDescription
            {
                RootEntityId = Id,
                RootEntityArchetype = Archetype,
                RootEntityName = Name,
                RootProp = propPath,

                EntityId = entity!.Id,
                EntityArchetype = entity.Archetype,
                EntityName = entity.Name,
                Prop = prop,
                Value = ModCalculator.Value(Graph, entity.GetMods(prop)) ?? Dice.Zero,
                BaseValue = ModCalculator.BaseValue(Graph, entity.GetMods(prop)) ?? Dice.Zero
            };

            propDesc.Mods = ModFilters.Active(entity.GetMods(prop))
                .Select(x => x.Describe())
                .Where(x => x != null)
                .Cast<ModDescription>()
                .ToList();

            return propDesc;
        }

        public Dice? Value(string path, bool calculate = false)
        {
            var propInfo = RpgReflection.ScanForModdableProperty(this, path, out var target);
            target ??= this;
            
            if (propInfo != null && !calculate)
            {
                var val = propInfo?.GetValue(target, null);
                if (val is Dice dice) return dice;
                if (val is int num) return num;
                return null;
            }

            return target is RpgObject rpgObj
                ? ModCalculator.Value(Graph, GetMods(propInfo?.Name ?? path))
                : null;
        }

        public Dice? InitialValue(string prop)
        {
            var mods = GetMods(prop);
            return ModCalculator.InitialValue(Graph, mods);
        }

        public Dice? BaseValue(string prop)
        {
            var mods = GetMods(prop);
            return ModCalculator.BaseValue(Graph, mods);
        }

        public int? ThresholdMinValue(string prop)
        {
            var mod = ModFilters.ActiveThreshold(GetMods(prop));
            return mod?.Behavior is Behaviors.Threshold threshold
                ? threshold.Min
                : null;
        }

        public int? ThresholdMaxValue(string prop)
        {
            var mod = ModFilters.ActiveThreshold(GetMods(prop));
            return mod?.Behavior is Behaviors.Threshold threshold
                ? threshold.Max
                : null;
        }

        public Dice? OriginalBaseValue(string prop)
        {
            var mods = GetMods(prop);
            return ModCalculator.OriginalBaseValue(Graph, mods);
        }

        public bool OverrideBaseValue(string prop, Dice? dice)
        {
            var overrides = GetMods(prop).Where(ModFilters.IsOverride).ToArray();
            if (overrides.Any()) RemoveMods(overrides);

            if (dice != null && dice != OriginalBaseValue(prop))
            {
                this.AddMod(new Override(), prop, dice.Value);
                return true;
            }

            return false;
        }

        internal void SetValue(string path, Dice? value)
        {
            var propInfo = RpgReflection.ScanForModdableProperty(this, path, out var pathEntity);
            if (propInfo?.SetMethod != null)
            {
                if (propInfo.PropertyType == typeof(int))
                {
                    var num = value?.Roll() ?? 0;
                    propInfo.SetValue(pathEntity, num);
                }
                else
                    propInfo.SetValue(pathEntity, value);
            }
        }

        public Prop? GetProp(string prop, RefType refType = RefType.Value, bool create = false)
            => Props.GetProp(Graph, this, prop, refType, create);

        public Prop[] GetProps()
            => Props.Values.ToArray();

        #endregion Props

        #region Mods

        public Mod[] GetMods()
            => Props.GetMods();

        public Mod[] GetMods(string prop)
            => Props.GetMods(prop);

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

                        Graph!.OnPropUpdated(new PropRef(prop.EntityId, prop.Name));
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
                            prop.Remove(mod);
                            Graph!.OnPropUpdated(new PropRef(prop.EntityId, prop.Name));

                            if (!prop.Mods.Any())
                                toRemove.Add(prop);
                        }
                    }

                    foreach (var prop in toRemove)
                        Props.Remove(prop.Name);
                }
                else
                    Graph!.RemoveMods([.. modGroup]);
            }
        }

        #endregion

        #region Refs

        public RpgObject? GetParentObject()
            => Graph?.GetParentObject(this);

        public bool IsParentObjectTo(RpgObject childObject)
            => IsParentObjectTo(childObject.Id);

        public bool IsParentObjectTo(string childObjectId)
            => Props.Values.Any(x => x.IsParentObjectTo(childObjectId));

        public T? GetChildObject<T>(string propName)
            where T : RpgObject
                => GetProp(propName, RefType.Child)?.GetChildObject<T>();

        public RpgObject[] GetChildObjects()
        {
            var res = new List<RpgObject>();
            foreach (var prop in GetProps().Where(x => x.RefType == RefType.Child || x.RefType == RefType.Children))
            {
                if (prop.RefType == RefType.Child)
                {
                    var child = prop.GetChildObject<RpgObject>();
                    if (child != null)
                        res.Add(child);
                }
                else
                {
                    res.AddRange(prop.GetChildObjects());
                }
            }    

            return res.ToArray();
        }

        public RpgObject[] GetChildObjects(string propName)
        {
            var res = new List<RpgObject>();
            var prop = GetProp(propName, RefType.Children, true);
            if (prop?.RefType != RefType.Children)
                throw new InvalidOperationException($"Prop {propName} not found or not RelType == RelType.Children");

            return prop.GetChildObjects();
        }

        public void AddChild(string propName, RpgObject? rpgObj)
        {
            var prop = GetProp(propName, RefType.Child, true)!;
            prop.AddRef(rpgObj);
        }

        public void AddChildren(string propName, params RpgObject[] rpgObjs)
        {
            var prop = GetProp(propName, RefType.Children, true)!;
            foreach (var rpgObj in rpgObjs)
                prop.AddRef(rpgObj);
        }

        public void RemoveChild(string propName, RpgObject rpgObj)
        {
            var prop = GetProp(propName, RefType.Child, true)!;
            prop.RemoveRef(rpgObj);
        }

        public void RemoveChildren(string propName, params RpgObject[] rpgObjs)
        {
            var prop = GetProp(propName, RefType.Children, true)!;
            foreach (var rpgObj in rpgObjs)
                prop.RemoveRef(rpgObj);
        }

        #endregion Refs

        #region States

        public State? GetState(string state)
            => States.ContainsKey(state) ? States[state] : null;

        public State? GetStateById(string id)
            => States.Values.FirstOrDefault(x => x.Id == id);

        public bool IsStateOn(string state)
            => GetState(state)?.IsOn ?? false;

        public void SetStateOn(string state)
            => GetState(state)?.Activate();

        public void SetStateOff(string state)
            => GetState(state)?.Deactivate();

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
            base.OnCreating(graph, entity);
            foreach (var modSet in ModSets.Values)
                modSet.OnCreating(graph, entity);

            OnCreatingOwnerIds();
            OnCreatingProperties();
            OnCreatingStates();
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph);

            foreach (var prop in Props.Values)
                prop.OnRestoring(graph);

            foreach (var state in States.Values)
                state.OnRestoring(graph);

            foreach (var modSet in ModSets.Values)
                modSet.OnRestoring(graph);

            foreach (var prop in Props.Values)
                prop.OnRestoring(graph);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();

            foreach (var state in States.Values)
                state.OnStartLifecycle();

            foreach (var modSet in ModSets.Values)
                modSet.OnStartLifecycle();

            foreach (var prop in Props.Values)
                prop.OnStartLifecycle();

            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();

            OnUpdateStateLifecycle();

            var modSetsToRemove = new List<ModSet>();
            foreach (var modSet in ModSets.Values)
            {
                var expiry = modSet.OnUpdateLifecycle();
                if (expiry == LifecycleExpiry.Destroyed)
                    modSetsToRemove.Add(modSet);
            }

            foreach (var remove in modSetsToRemove)
                RemoveModSet(remove.Id);

            foreach (var prop in Props.Values)
                prop.OnUpdateLifecycle();

            return Expiry;
        }

        public override string ToString()
        {
            return $"{Archetype} {Id}";
        }

        private void OnCreatingOwnerIds()
        {
            foreach (var propertyInfo in this.GetType().GetProperties().Where(x => x.PropertyType.IsAssignableTo(typeof(RpgObject))))
            {
                var rpgObj = propertyInfo.GetValue(this, null) as RpgObject;
                if (rpgObj != null) rpgObj.OwnerId = Id;
            }
        }

        private void OnCreatingProperties()
        {
            if (Graph == null)
                throw new InvalidOperationException("Graph is null");

            foreach (var propInfo in RpgReflection.ScanForChildProperties(this))
            {
                if (!Props.ContainsKey(propInfo.Name))
                {
                    var prop = new Prop(Id, propInfo.Name, RefType.Child);
                    Props.Add(propInfo.Name, prop);
                }

                Props[propInfo.Name].OnCreating(Graph, this);
            }

            foreach (var propInfo in RpgReflection.ScanForChildrenProperties(this))
            {
                if (!Props.ContainsKey(propInfo.Name))
                {
                    var prop = new Prop(Id, propInfo.Name, RefType.Children);
                    Props.Add(propInfo.Name, prop);
                }
             
                Props[propInfo.Name].OnCreating(Graph, this);
            }

            foreach (var propInfo in RpgReflection.ScanForModdableProperties(this))
            {
                var val = this.PropertyValue(propInfo.Name, out var propExists);
                if (val != null)
                {

                    if (val is Dice dice)
                        AddMods(new Initial(Id, propInfo.Name, dice));
                    else if (val is int i)
                        AddMods(new Initial(Id, propInfo.Name, i));

                    var (min, max) = propInfo.GetPropertyThresholds();
                    if (min != null || max != null)
                        AddMods(new Threshold(Id, propInfo.Name, min ?? int.MinValue, max ?? int.MaxValue));
                }
            }
        }

        private void OnCreatingStates()
        {
            var types = RpgTypeScan.ForTypes<State>()
                .Where(x => IsOwnerStateType(this, x));

            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [this])!;
                if (this.IsA(state.OwnerArchetype!))
                {
                    state.OnCreating(Graph);
                    States.Add(state.Name, state);
                }
            }
        }

        private bool IsOwnerStateType(RpgObject entity, Type? stateType)
        {
            while (stateType != null)
            {
                if (stateType.IsGenericType)
                {
                    var genericTypes = stateType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                stateType = stateType.BaseType;
            }

            return false;
        }
    }
}
