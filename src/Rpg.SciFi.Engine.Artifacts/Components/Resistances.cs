using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;
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

    public class Resistances : Modifiable
    {
        public Resistances() { }

        public Resistances(int baseImpact, int basePierce, int baseBlast, int baseBurn, int baseEnergy)
        {
            BaseImpact = baseImpact;
            BasePierce = basePierce;
            BaseBlast = baseBlast;
            BaseHeat = baseBurn;
            BaseEnergy = baseEnergy;
        }
        
        [JsonProperty] public int BaseImpact { get; protected set; }
        [JsonProperty] public int BasePierce { get; protected set; }
        [JsonProperty] public int BaseBlast { get; protected set; }
        [JsonProperty] public int BaseHeat { get; protected set; }
        [JsonProperty] public int BaseEnergy { get; protected set; }

        [Modifiable("Impact", "Impact")] public int Impact { get => BaseImpact + ModifierRoll(nameof(BaseImpact)); }
        [Modifiable("Pierce", "Pierce")] public int Pierce { get => BasePierce + ModifierRoll(nameof(BasePierce));}
        [Modifiable("Blast", "Blast")] public int Blast { get => BaseBlast + ModifierRoll(nameof(BaseBlast)); }
        [Modifiable("Heat", "Heat")] public int Heat { get => BaseHeat + ModifierRoll(nameof(BaseHeat)); }
        [Modifiable("Energy", "Energy")] public int Energy { get => BaseEnergy + ModifierRoll(nameof(BaseEnergy)); }
    }
}
