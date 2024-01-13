using Newtonsoft.Json;
using Rpg.Sys.Modifiers;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rpg.Sys
{
    public abstract class ModdableObject : INotifyPropertyChanged, IModSubscriber
    {
        protected Graph Graph { get; set; }

        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string[] Is { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModdableObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
        }

        public bool IsA(string type) => Is.Contains(type);

        public virtual void OnAdd(Graph graph) => Graph = graph;

        public virtual Modifier[] OnSetup()
        {
            var mods = new List<Modifier>();

            foreach (var propInfo in this.ModdableProperties())
            {
                var val = this.PropertyValue(propInfo.Name);
                if (val != null)
                {
                    if (val is Dice && ((Dice)val) != Dice.Zero)
                        mods.Add(BaseModifier.CreateByPath(this, (Dice)val, propInfo.Name));
                    else if (val is int && ((int)val) != 0)
                        mods.Add(BaseModifier.CreateByPath(this, (int)val, propInfo.Name));
                }
            }

            return mods.ToArray();
        }

        public void CallPropertyChanged(string prop) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public Dice? GetModdableProperty(string prop)
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

        public void SetModdableProperty(string prop, Dice dice)
        {
            this.PropertyValue(prop, dice);
            CallPropertyChanged(prop);
        }

        protected void NotifyPropertyChanged(string prop)
            => Graph?.Mods?.NotifyPropertyChanged(Id, prop);
    }
}
