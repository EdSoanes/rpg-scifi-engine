using Newtonsoft.Json;
using Rpg.Sys.Modifiers;
using System.ComponentModel;

namespace Rpg.Sys.Moddable
{
    public abstract class ModObject : INotifyPropertyChanged
    {
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
            PropStore = new ModObjectPropStore(Id);
        }

        public void AddMod(Modifier mod)
        {
            PropStore.Add(this, mod);
            SetModdableValue(mod.Target.EntityId, mod.Target.Prop);
        }

        public bool IsA(string type) => Is.Contains(type);

        public void BuildGraph()
        {
            ModGraph.Current.SetContext(this);

            foreach (var entity in this.Traverse(true).Where(x => !x.IsInitialized))
            {
                entity.InitializeBaseValues();
                entity.OnInitialize();
                entity.IsInitialized = true;
            }

            UpdateProps();
        }

        protected virtual void OnInitialize() { }

        public void UpdateGraph()
            => UpdateEntityGraph(ModGraph.Current.GetContext());

        public void UpdateSubgraph()
            => UpdateEntityGraph(this);

        public void UpdateProps()
        {
            foreach (var modProp in PropStore)
                UpdateProp(modProp.Prop);
        }

        public void DeepUpdateProps()
        {
            UpdateSubgraph();
            UpdateProps();
        }

        public void UpdateProp(string prop)
        {
            var oldValue = GetModdableValue(prop);
            var newValue = PropStore.Evaluate(prop) ?? Dice.Zero;

            if (oldValue == null || oldValue != newValue)
            {
                this.PropertyValue(prop, newValue);
                CallPropertyChanged(prop);
            }
        }

        public void DeepUpdateProp(string prop)
        {
            foreach (var propRef in ModGraph.Current.AffectedByProps(Id, prop))
            {
                var entity = ModGraph.Current.GetEntity<ModObject>(propRef.EntityId)!;
                entity.UpdateProp(propRef.Prop);
            }

            UpdateProp(prop);
        }

        private void UpdateEntityGraph(ModObject rootEntity)
        {
            var affectedBy = new List<ModObjectPropRef>();

            foreach (var entity in rootEntity.Traverse(true))
                affectedBy.Merge(entity.PropStore.AffectedBy());

            foreach (var propRef in affectedBy)
                SetModdableValue(propRef.EntityId, propRef.Prop);
        }

        private void InitializeBaseValues()
        {
            foreach (var propInfo in this.ModdableProperties())
            {
                var val = this.PropertyValue(propInfo.Name);
                if (val != null)
                {
                    if (val is Dice && (Dice)val != Dice.Zero)
                        PropStore.Init(this, BaseValueModifier.Create(this, (Dice)val, propInfo.Name));
                    else if (val is int && (int)val != 0)
                        PropStore.Init(this, BaseValueModifier.Create(this, (int)val, propInfo.Name));
                }
            }
        }

        private void SetModdableValue(Guid entityId, string prop)
        {
            var entity = ModGraph.Current.GetEntity<ModObject>(entityId);
            entity?.UpdateProp(prop);
        }

        public Dice? GetModdableValue(string prop)
        {
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
            => ModGraph.Current?.Notify.Send(Id, prop);
    }
}
