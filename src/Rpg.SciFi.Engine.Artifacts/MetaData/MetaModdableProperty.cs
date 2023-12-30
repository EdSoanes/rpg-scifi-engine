using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModdableProperty
    {
        private Dice? _dice = null;

        public Guid EntityId { get; set; }
        public string Prop { get; set; }
        public bool IsDiceProperty { get; set; }
        public Dice Dice 
        { 
            get => _dice ?? "0";
            set => _dice = value;
        }

        public bool DiceIsSet
        {
            get => _dice != null;
            set
            {
                if (!value)
                    _dice = null;
            }
        }

        public List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonConstructor] private MetaModdableProperty() { }

        public MetaModdableProperty(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }
    }
}
