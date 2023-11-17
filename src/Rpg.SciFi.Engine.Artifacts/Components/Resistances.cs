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
        protected Resistance(string name, string? description)
        {
            Name = name;
            Description = description;
        }

        public Resistance(string name, string? description, int baseValue)
        {
            Name = name;
            Description = description;
            BaseValue = baseValue;
        }

        [JsonProperty] public virtual int BaseValue {get; protected set;}
        public virtual int Value => BaseValue + ModifierRoll(nameof(Value));
    }

    public class CompositeResistance : Resistance
    {
        [JsonProperty] private Resistance[] _resistances { get; set;} = new Resistance[0];

        public CompositeResistance() { }
        public CompositeResistance(string name, string? description, params Resistance[] resistances)
            : base(name, description)
        {
            _resistances = resistances;
        }

        [JsonProperty]
        public override int BaseValue 
        { 
            get { return _resistances.Sum(x => x.BaseValue); } 
            protected set { throw new ArgumentException("Cannot set BaseValue"); } 
        }

        public override int Value => BaseValue + _resistances.Sum(x => x.ModifierRoll(nameof(Value)));
    }

    public class ResistanceSignature
    {
        public ResistanceSignature() { }

        public ResistanceSignature(int impact, int pierce, int blast, int burn, int energy)
        {
            Impact = new Resistance(nameof(Impact), nameof(Impact), impact);
            Pierce = new Resistance(nameof(Pierce), nameof(Pierce), pierce);
            Blast = new Resistance(nameof(Blast), nameof(Blast), blast);
            Burn = new Resistance(nameof(Burn), nameof(Burn), burn);
            Energy = new Resistance(nameof(Energy), nameof(Energy), energy);
        }

        public ResistanceSignature(Resistance impact, Resistance pierce, Resistance blast, Resistance burn, Resistance energy)
        {
            Impact = impact;
            Pierce = pierce;
            Blast = blast;
            Burn = burn;
            Energy = energy;
        }

        [JsonProperty] public Resistance Impact { get; protected set; } = new Resistance(nameof(Impact), nameof(Impact), 0);
        [JsonProperty] public Resistance Pierce { get; protected set; } = new Resistance(nameof(Pierce), nameof(Pierce), 0);
        [JsonProperty] public Resistance Blast { get; protected set; } = new Resistance(nameof(Blast), nameof(Blast), 0);
        [JsonProperty] public Resistance Burn { get; protected set; } = new Resistance(nameof(Burn), nameof(Burn), 0);
        [JsonProperty] public Resistance Energy { get; protected set; } = new Resistance(nameof(Energy), nameof(Energy), 0);
    }
}
