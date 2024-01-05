using Newtonsoft.Json;
using Rpg.Sys.Modifiers;
using System.ComponentModel;

namespace Rpg.Sys
{
    public abstract class ModdableObject : INotifyPropertyChanged, IModSubscriber
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModdableObject()
        {
            Name = GetType().Name;
        }

        public virtual Modifier[] SetupModdableProperties()
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
