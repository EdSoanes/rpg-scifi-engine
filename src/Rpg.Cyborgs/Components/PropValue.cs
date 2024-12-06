using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.Components
{
    public class PropValue : RpgComponent
    {
        [Integer]
        [JsonProperty] 
        public int Value { get; protected set; }

        [Integer(Ignore = true)]
        public int InitValue { get => this.InitialValue(nameof(Value))?.Roll() ?? 0; }

        [Integer(Ignore = true)]
        public int BaseValue { get => this.BaseValue(nameof(Value))?.Roll() ?? 0; }

        [Integer(Ignore = true)]
        public int OriginalBaseValue { get => this.OriginalBaseValue(nameof(Value))?.Roll() ?? 0; }

        [JsonConstructor] private PropValue() { }

        public PropValue(int value = 0)
            : base()
        {
            Value = value;
        }

        public PropValue(string name)
            : base(name)
        {
        }

        public PropValue(string name, int value = 0)
            : base(name) 
        {
            Value = value;
        }
    }
}
