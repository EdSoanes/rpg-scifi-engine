using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects
{
    public abstract class RpgObject : RpgLifecycleObject, INotifyPropertyChanged
    {
        [JsonInclude] public ModSetDictionary ModSets { get; set; }
        [JsonInclude] public PropsDictionary Props { get; set; }
        [JsonInclude] public StatesDictionary States { get; set; }

        [JsonInclude] public string Id { get; private set; }

        [JsonInclude] public string Archetype { get; private set; }

        [JsonInclude] public string Name { get; protected set; }

        [JsonInclude] public string[] Archetypes { get; private set; }

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

        public Prop? GetProp(string? prop, RefType refType = RefType.Value, bool create = false)
        {
            if (string.IsNullOrEmpty(prop))
                return null;

            if (Props.ContainsKey(prop))
                return Props[prop];

            if (create)
            {
                var created = new Prop(Id, prop, refType);
                if (Graph != null)
                {
                    created.OnCreating(Graph, this);
                    created.OnTimeBegins();
                    created.OnStartLifecycle();
                }

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

                        Graph!.OnPropUpdated(new PropRef(prop.EntityId, prop.Name));
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
                        Graph!.OnPropUpdated(mod.Target);
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
                        Graph!.OnPropUpdated(mod.Target);
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
                        Graph!.OnPropUpdated(mod.Target);
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
                        Graph!.OnPropUpdated(mod.Target);
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
            => GetState(state)?.On();

        public void SetStateOff(string state)
            => GetState(state)?.Off();

        public ModSet CreateStateInstance(string state, SpanOfTime? spanOfTime = null)
            => GetState(state)!.CreateInstance(spanOfTime);

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

            OnCreatingProperties();

            var states = State.CreateOwnerStates(this);
            foreach (var state in states)
            {
                state.OnCreating(Graph);
                States.Add(state.Name, state);
            }
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
    }
}
