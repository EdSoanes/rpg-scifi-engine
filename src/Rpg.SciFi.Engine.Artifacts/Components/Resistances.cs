using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Resistance : Modifiable
    {
        public Resistance() { }
        public Resistance(string name, string? description, int baseValue)
        {
            Name = name;
            Description = description;
            BaseValue = baseValue;
        }

        [JsonProperty] public int BaseValue {get; private set;}
        public int Value => BaseValue + ModifierRoll(nameof(Value));
    }

    public class ResistanceSignature
    {
        [JsonProperty] public Resistance Impact { get; protected set; } = new Resistance(nameof(Impact), nameof(Impact), 0);
        [JsonProperty] public Resistance Pierce { get; protected set; } = new Resistance(nameof(Pierce), nameof(Pierce), 0);
        [JsonProperty] public Resistance Blast { get; protected set; } = new Resistance(nameof(Blast), nameof(Blast), 0);
        [JsonProperty] public Resistance Burn { get; protected set; } = new Resistance(nameof(Burn), nameof(Burn), 0);
        [JsonProperty] public Resistance Energy { get; protected set; } = new Resistance(nameof(Energy), nameof(Energy), 0);
    }
}
