using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
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
        [JsonProperty] public ModPropStore PropStore { get; private set; } = new ModPropStore();
        [JsonProperty] public ModSetStore ModSetStore { get; private set; } = new ModSetStore();
        [JsonProperty] public ModStateStore StateStore { get; private set; } = new ModStateStore();
        [JsonProperty] protected bool IsCreated { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
        }

        internal void AddMod(Mod mod)
            => Graph!.Context!.PropStore.Add(mod);

        public ModSet? AddModSet(ModDuration duration, params Mod[] mods)
        {
            var modSet = new ModSet(duration, mods);
            var added = ModSetStore.Add(modSet);
            if (added)
            {
                foreach (var mod in mods)
                    AddMod(mod);

                return modSet;
            }

            return null;
        }

        public ModSet? GetModSet(Guid id)
            => ModSetStore.ModSets.FirstOrDefault(x => x.Id == id);

        public ModSet? GetModSet(string name)
            => ModSetStore.ModSets.FirstOrDefault(x => x.Name == name);

        public bool AddModSet(ModSet modSet)
            => ModSetStore.Add(modSet);

        public void RemoveModSet(Guid modSetId)
            => ModSetStore.Remove(modSetId);

        public void RemoveModSet(string name)
            => ModSetStore.Remove(name);

        public bool IsA(string type) => Is.Contains(type);

        public void TriggerUpdate()
            => OnTurnChanged(Graph!.Turn);

        internal void TriggerUpdate(ModPropRef propRef)
        {
            var propsAffected = PropStore.PropsAffectedBy(propRef);
            foreach (var prop in propsAffected)
            {
                var entity = Graph!.GetEntity<ModObject>(prop.EntityId);
                entity?.SetPropValue(prop.Prop);
            }
        }

        internal ModPropDescription Describe(ModPropRef propRef)
            => new ModPropDescription(Graph!, this, propRef);

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

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            Graph = graph;
            PropStore.OnGraphCreating(Graph, this);
            ModSetStore.OnGraphCreating(Graph, this);
            StateStore.OnGraphCreating(Graph, this);

            if (!IsCreated)
            {
                foreach (var propInfo in this.ModdableProperties())
                {
                    var val = this.PropertyValue(propInfo.Name);
                    if (val != null)
                    {
                        if (val is Dice dice && dice != Dice.Zero)
                            PropStore.Add(BaseValueMod.Create(this, propInfo.Name, dice));
                        else if (val is int i && i != 0)
                            PropStore.Add(BaseValueMod.Create(this, propInfo.Name, i));
                    }
                }

                OnCreate();
                IsCreated = true;
            }
        }

        protected virtual void OnCreate() { }

        public void OnTurnChanged(int turn)
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnTurnChanged(turn);
                entity.PropStore.OnTurnChanged(turn);
                entity.StateStore.OnTurnChanged(turn);
            }

            UpdateProps();
        }

        public void OnBeginEncounter()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnBeginEncounter();
                entity.PropStore.OnBeginEncounter();
                entity.StateStore.OnBeginEncounter();
            }

            UpdateProps();
        }

        public void OnEndEncounter()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnEndEncounter();
                entity.PropStore.OnEndEncounter();
                entity.StateStore.OnEndEncounter();
            }

            UpdateProps();
        }
    }
}
