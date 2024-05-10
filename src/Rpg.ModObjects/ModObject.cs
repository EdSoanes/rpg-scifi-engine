using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.ComponentModel;

namespace Rpg.ModObjects
{
    public abstract class ModObject : INotifyPropertyChanged
    {
        protected ModGraph? Graph { get; private set; }

        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string[] Is { get; private set; }
        [JsonProperty] public ModObjectPropStore PropStore { get; set; }
        [JsonProperty] protected bool IsInitialized { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
            PropStore = new ModObjectPropStore();
        }

        public void AddMod(Mod mod)
            => PropStore.Add(mod);

        public bool IsA(string type) => Is.Contains(type);

        public ModGraph BuildGraph()
        {
            var graph = new ModGraph(this);
            foreach (var entity in this.Traverse(true))
            {
                entity.Initialize(graph);
                if (!entity.IsInitialized)
                {
                    entity.OnBuildGraph();
                    entity.IsInitialized = true;
                }
            }

            OnPropsUpdated();

            return graph;
        }

        protected virtual void OnBuildGraph() { }

        public void RemoveExpiredProps()
        {
            foreach (var entity in Graph!.Context!.Traverse(true))
                entity.PropStore.RemoveExpiredProps();
        }

        public void OnPropsUpdated()
        {
            var affectedBy = new List<ModObjectPropRef>();

            foreach (var entity in Graph!.Context!.Traverse(true))
                affectedBy.Merge(entity.PropStore.AffectedByProps());

            foreach (var propRef in affectedBy)
            {
                var entity = Graph!.GetEntity<ModObject>(propRef.EntityId);
                entity?.SetPropValue(propRef.Prop);
            }
        }

        public void OnPropUpdated(ModObjectPropRef propRef)
        {
            var propsAffected = PropStore.PropsAffectedBy(propRef);
            foreach (var prop in propsAffected)
            {
                var entity = Graph!.GetEntity<ModObject>(prop.EntityId);
                entity?.SetPropValue(prop.Prop);
            }
        }

        private void SetPropValue(string prop)
        {
            var oldValue = GetModdableValue(prop);
            var newValue = PropStore.Calculate(prop);

            if (oldValue == null || oldValue != newValue)
            {
                this.PropertyValue(prop, newValue);
                CallPropertyChanged(prop);
            }
        }

        private void Initialize(ModGraph graph)
        {
            Graph = graph;
            PropStore.Initialize(Graph, this);

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

        private void SetModdableValue(ModObjectPropRef propRef)
        {
            var entity = Graph!.GetEntity<ModObject>(propRef.EntityId);
            entity?.SetPropValue(propRef.Prop);
        }

        public Dice? GetModdableValue(string? prop)
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
          //  => Graph?.Notify.Send(Id, prop);
    }
}
