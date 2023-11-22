using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Resistances : Modifiable
    {
        public Resistances() 
        {
            Name = nameof(Resistances);
        }

        public Resistances(int baseImpact, int basePierce, int baseBlast, int baseBurn, int baseEnergy)
            : this()
        {

            BaseImpact = baseImpact;
            BasePierce = basePierce;
            BaseBlast = baseBlast;
            BaseHeat = baseBurn;
            BaseEnergy = baseEnergy;
        }
        
        [JsonProperty] public virtual int BaseImpact { get; protected set; }
        [JsonProperty] public virtual int BasePierce { get; protected set; }
        [JsonProperty] public virtual int BaseBlast { get; protected set; }
        [JsonProperty] public virtual int BaseHeat { get; protected set; }
        [JsonProperty] public virtual int BaseEnergy { get; protected set; }

        [Moddable] public virtual int Impact { get => BaseImpact + ModifierRoll(nameof(BaseImpact)); }
        [Moddable] public virtual int Pierce { get => BasePierce + ModifierRoll(nameof(BasePierce));}
        [Moddable] public virtual int Blast { get => BaseBlast + ModifierRoll(nameof(BaseBlast)); }
        [Moddable] public virtual int Heat { get => BaseHeat + ModifierRoll(nameof(BaseHeat)); }
        [Moddable] public virtual int Energy { get => BaseEnergy + ModifierRoll(nameof(BaseEnergy)); }
    }

    public class CompositeResistances : Resistances
    {
        [JsonProperty] private Resistances[] _resistances { get; set; } = new Resistances[0];

        public CompositeResistances()
        {
            Name = nameof(Resistances);
        }

        public CompositeResistances(params Resistances[] resistances)
            : this()
        {
            _resistances = resistances;
        }

        [JsonProperty] public override int BaseImpact { get => _resistances.Sum(x => x.BaseImpact); protected set => throw new ArgumentException(nameof(BaseImpact)); }
        [JsonProperty] public override int BasePierce { get => _resistances.Sum(x => x.BasePierce); protected set => throw new ArgumentException(nameof(BasePierce)); }
        [JsonProperty] public override int BaseBlast { get => _resistances.Sum(x => x.BaseBlast); protected set => throw new ArgumentException(nameof(BaseBlast)); }
        [JsonProperty] public override int BaseHeat { get => _resistances.Sum(x => x.BaseHeat); protected set => throw new ArgumentException(nameof(BaseHeat)); }
        [JsonProperty] public override int BaseEnergy { get => _resistances.Sum(x => x.BaseEnergy); protected set => throw new ArgumentException(nameof(BaseEnergy)); }

        [Moddable] public override int Impact { get => BaseImpact + _resistances.Sum(x => x.Impact); }
        [Moddable] public override int Pierce { get => BasePierce + _resistances.Sum(x => x.Pierce); }
        [Moddable] public override int Blast { get => BaseBlast + _resistances.Sum(x => x.Blast); }
        [Moddable] public override int Heat { get => BaseHeat + _resistances.Sum(x => x.Heat); }
        [Moddable] public override int Energy { get => BaseEnergy + _resistances.Sum(x => x.Energy); }
    }
}
