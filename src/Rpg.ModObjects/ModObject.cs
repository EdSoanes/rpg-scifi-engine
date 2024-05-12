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
        [JsonProperty] protected bool IsInitialized { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
        }

        internal void AddMod(Mod mod)
            => Graph!.Context!.PropStore.Add(mod);

        public ModSet AddModSet(ModDuration duration, params Mod[] mods)
        {
            var modSet = new ModSet(duration, mods);
            ModSetStore.Add(modSet);

            foreach (var mod in mods)
                AddMod(mod);

            return modSet;
        }

        public void RemoveModSet(Guid modSetId)
        {
            ModSetStore.Remove(modSetId);
        }

        public bool IsA(string type) => Is.Contains(type);

        public ModGraph BuildGraph()
        {
            var graph = new ModGraph(this);
            foreach (var entity in this.Traverse())
                entity.Initialize(graph);

            foreach (var entity in this.Traverse(true))
            {
                if (!entity.IsInitialized)
                {
                    entity.OnInitialize();
                    entity.IsInitialized = true;
                }
            }

            graph.Initialize();

            return graph;
        }

        private void Initialize(ModGraph graph)
        {
            Graph = graph;
            PropStore.Initialize(Graph, this);
            ModSetStore.Initialize(Graph, this);

            if (!IsInitialized)
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
            }
        }

        protected virtual void OnInitialize() { }

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

        private void UpdateProps()
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
                CallPropertyChanged(prop);
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

        public void CallPropertyChanged(string prop)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        protected void NotifyPropertyChanged(string prop)
        { }

        public void OnTurnChanged(int turn)
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnTurnChanged(turn);
                entity.PropStore.OnTurnChanged(turn);
            }

            UpdateProps();
        }

        public void OnEncounterStarted()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnEncounterStarted();
                entity.PropStore.OnEncounterStarted();
            }

            UpdateProps();
        }

        public void OnEncounterEnded()
        {
            foreach (var entity in this.Traverse())
            {
                entity.ModSetStore.OnEncounterEnded();
                entity.PropStore.OnEncounterEnded();
            }

            UpdateProps();
        }
    }
}
