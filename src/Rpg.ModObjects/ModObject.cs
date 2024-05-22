using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Stores;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class ModObject : INotifyPropertyChanged, ITemporal
    {
        protected ModGraph? Graph { get; private set; }

        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string[] Is { get; private set; }
        [JsonProperty] protected ModPropStore PropStore { get; private set; } = new ModPropStore();
        [JsonProperty] protected ModSetStore ModSetStore { get; private set; } = new ModSetStore();
        [JsonProperty] protected ModStateStore StateStore { get; private set; } = new ModStateStore();
        [JsonProperty] protected bool IsCreated { get; set; }
        [JsonProperty] public ModCmdDescriptor[] Commands { get; private set; } = new ModCmdDescriptor[0];

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
        }

        internal IEnumerable<ModPropRef> GetPropsThatAffect(string prop)
            => PropStore.GetPropsThatAffect(prop);

        internal IEnumerable<ModPropRef> GetPropsAffectedBy(string prop)
            => PropStore.GetPropsAffectedBy(new ModPropRef(Id, prop));

        internal Mod[] GetMods(bool filtered = true)
            => PropStore.GetMods(filtered);

        internal Mod[] GetMods(string prop, bool filtered = true)
            => PropStore.GetMods(prop, filtered);

        internal ModProp? GetModProp(string? prop, bool create = false)
        {
            if (string.IsNullOrEmpty(prop))
                return null;

            var modProp = PropStore[prop];
            if (modProp == null && create)
                return PropStore.Create(prop);

            return modProp;
        }

        internal ModProp[] GetModProps()
            => PropStore.Get();

        public bool RemoveModProp(string prop)
            => PropStore.Remove(prop);

        internal void AddMod(Mod mod)
            => Graph?.Context?.PropStore?.Add(mod);

        public void RemoveMods(params Mod[] mods)
            => Graph?.Context?.PropStore?.Remove(mods);

        internal ModSet[] GetModSets()
            => ModSetStore.Get();

        internal ModSet? AddModSet(string name, ModDuration duration, params Mod[] mods)
        {
            var modSet = new ModSet(name, duration, mods);
            return ModSetStore.Add(modSet)
                ? modSet
                : null;
        }

        internal bool AddModSet(ModSet modSet)
            => ModSetStore.Add(modSet);
        public ModSet? GetModSet(Guid id)
            => ModSetStore.Get().FirstOrDefault(x => x.Id == id);

        public ModSet? GetModSet(string name)
            => ModSetStore.Get().FirstOrDefault(x => x.Name == name);

        public void RemoveModSet(Guid modSetId)
            => ModSetStore.Remove(modSetId);

        public void RemoveModSet(string name)
            => ModSetStore.Remove(name);

        public ModObject AddState(ModState state)
        {
            StateStore.Add(state);
            return this;
        }

        public string[] States { get => StateStore.AllStates; }
        public string[] ActiveStates { get => StateStore.ActiveStates; }

        public bool ManuallyActivateState(string state)
            => StateStore.ManuallyActivateState(state);

        public bool ManuallyDeactivateState(string state)
            => StateStore.ManuallyDeactivateState(state);

        public bool IsA(string type) => Is.Contains(type);

        public void TriggerUpdate()
            => OnTurnChanged(Graph!.Turn);

        internal void TriggerUpdate(ModPropRef propRef)
        {
            var propsAffected = PropStore.GetPropsAffectedBy(propRef);
            foreach (var prop in propsAffected)
            {
                var entity = Graph!.GetEntity<ModObject>(prop.EntityId);
                entity?.SetPropValue(prop.Prop);
            }
        }

        internal ModObjectPropDescription Describe(string prop)
            => new ModObjectPropDescription(Graph!, this, prop);

        internal void UpdateProps()
        {
            var affectedBy = new List<ModPropRef>();
            foreach (var entity in this.Traverse(true))
                affectedBy.Merge(entity.PropStore.AffectedByProps());

            foreach (var propRef in affectedBy)
            {
                var entity = Graph!.GetEntity<ModObject>(propRef.EntityId);
                entity?.SetPropValue(propRef.Prop);
            }
        }

        public void SetPropValue(string prop)
        {
            var oldValue = GetPropValue(prop);
            var newValue = PropStore.Calculate(prop);

            if (oldValue == null || oldValue != newValue)
            {
                this.PropertyValue(prop, newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }

        public Dice? GetPropValue(string? prop)
        {
            if (string.IsNullOrEmpty(prop))
                return null;

            var val = this.PropertyValue(prop);
            if (val != null)
            {
                if (val is Dice)
                    return (Dice)val;
                else if (val is int)
                    return (int)val;
            }

            return null;
        }

        public Dice? CalculateInitialValue(string prop)
            => PropStore.CalculateInitialValue(prop);

        public Dice? CalculateBaseValue(string prop)
            => PropStore.CalculateBaseValue(prop);

        internal Dice? CalculatePropValue(string? prop, ModType? modType = null, string? modName = null)
        {
            if (!string.IsNullOrEmpty(prop))
                return PropStore.Calculate(prop, modType, modName);

            return null;
        }

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            PropStore.OnGraphCreating(Graph, this);
            ModSetStore.OnGraphCreating(Graph, this);
            if (!IsCreated)
            {
                Commands = this.ModActionDescriptors();

                foreach (var propInfo in this.ModdableProperties())
                {
                    var val = this.PropertyValue(propInfo.Name);
                    if (val != null)
                    {
                        if (val is Dice dice && dice != Dice.Zero)
                            PropStore.Add(BaseInitMod.Create(this, propInfo.Name, dice));
                        else if (val is int i && i != 0)
                            PropStore.Add(BaseInitMod.Create(this, propInfo.Name, i));
                    }
                }

                OnCreate();
                IsCreated = true;
            }

            StateStore.OnGraphCreating(Graph, this);
        }

        protected virtual void OnCreate() { }

        public virtual void OnTurnChanged(int turn)
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnTurnChanged(turn);
                entity.PropStore.OnTurnChanged(turn);
            }

            UpdateProps();

            foreach (var entity in this.Traverse())
            {
                entity.StateStore.OnTurnChanged(turn);
                entity.ModSetStore.OnTurnChanged(turn);
                entity.PropStore.OnTurnChanged(turn);
            }

            UpdateProps();
        }

        public virtual void OnBeginEncounter()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnBeginEncounter();
                entity.PropStore.OnBeginEncounter();
            }

            UpdateProps();

            foreach (var entity in this.Traverse())
            {
                entity.StateStore.OnBeginEncounter();
                entity.ModSetStore.OnBeginEncounter();
                entity.PropStore.OnBeginEncounter();
            }

            UpdateProps();
        }

        public virtual void OnEndEncounter()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnEndEncounter();
                entity.PropStore.OnEndEncounter();
            }

            UpdateProps();

            foreach (var entity in this.Traverse())
            {
                entity.StateStore.OnEndEncounter();
                entity.ModSetStore.OnEndEncounter();
                entity.PropStore.OnEndEncounter();
            }

            UpdateProps();
        }
    }
}
