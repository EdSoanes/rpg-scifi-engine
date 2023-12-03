using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

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
            this.Mod(() => BaseImpact, () => Impact).IsBase().Apply();
            this.Mod(() => BasePierce, () => Pierce).IsBase().Apply();
            this.Mod(() => BaseBlast, () => Blast).IsBase().Apply();
            this.Mod(() => BaseHeat, () => Heat).IsBase().Apply();
            this.Mod(() => BaseEnergy, () => Energy).IsBase().Apply();
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

        public override int Impact { get => _resistances.Sum(x => x.Impact); }
        public override int Pierce { get => _resistances.Sum(x => x.Pierce); }
        public override int Blast { get => _resistances.Sum(x => x.Blast); }
        public override int Heat { get => _resistances.Sum(x => x.Heat); }
        public override int Energy { get => _resistances.Sum(x => x.Energy); }
    }
}
