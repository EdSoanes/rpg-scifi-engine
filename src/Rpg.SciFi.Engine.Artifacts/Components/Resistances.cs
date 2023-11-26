using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Resistances : Entity
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
        
        [JsonProperty] public virtual int BaseImpact { get; protected set; }
        [JsonProperty] public virtual int BasePierce { get; protected set; }
        [JsonProperty] public virtual int BaseBlast { get; protected set; }
        [JsonProperty] public virtual int BaseHeat { get; protected set; }
        [JsonProperty] public virtual int BaseEnergy { get; protected set; }

        [Moddable] public virtual int Impact { get => this.Resolve(nameof(BaseImpact)); }
        [Moddable] public virtual int Pierce { get => this.Resolve(nameof(BasePierce));}
        [Moddable] public virtual int Blast { get => this.Resolve(nameof(BaseBlast)); }
        [Moddable] public virtual int Heat { get => this.Resolve(nameof(BaseHeat)); }
        [Moddable] public virtual int Energy { get => this.Resolve(nameof(BaseEnergy)); }

        [Setup]
        public void Setup()
        {
            this.AddMod(x => x.BaseImpact, x => x.Impact);
            this.AddMod(x => x.BasePierce, x => x.Pierce);
            this.AddMod(x => x.BaseBlast, x => x.Blast);
            this.AddMod(x => x.BaseHeat, x => x.Heat);
            this.AddMod(x => x.BaseEnergy, x => x.Energy);
        }
    }

    public class CompositeResistances : Resistances
    {
        [JsonProperty] private Resistances[] _resistances { get; set; } = new Resistances[0];

        public CompositeResistances(params Resistances[] resistances)
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
