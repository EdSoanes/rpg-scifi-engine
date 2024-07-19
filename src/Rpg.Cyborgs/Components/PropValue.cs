using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Components
{
    public class PropValue : RpgComponent
    {
        [Integer]
        [JsonProperty] 
        public int Value { get; private set; }

        [Integer(Ignore = true)]
        public int InitValue { get => Graph?.CalculateInitialPropValue(this, nameof(Value))?.Roll() ?? 0; }

        [Integer(Ignore = true)]
        public int BaseValue { get => Graph?.CalculateBasePropValue(this, nameof(Value))?.Roll() ?? 0; }

        [JsonConstructor] private PropValue() { }

        public PropValue(int value = 0)
            : base()
        {
            Value = value;
        }

        public PropValue(string name, int value = 0)
            : base(name) 
        {
            Value = value;
        }
    }
}
