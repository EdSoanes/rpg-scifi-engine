using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Resistances : ModdableObject
    { 
        private readonly int _baseImpact;
        private readonly int _basePierce;
        private readonly int _baseBlast;
        private readonly int _baseBurn;
        private readonly int _baseEnergy;

        public Resistances() { }

        public Resistances(int baseImpact, int basePierce, int baseBlast, int baseBurn, int baseEnergy)
        {
            _baseImpact = baseImpact;
            _basePierce = basePierce;
            _baseBlast = baseBlast;
            _baseBurn = baseBurn;
            _baseEnergy = baseEnergy;
        }

        [Moddable] public virtual int BaseImpact { get => Resolve(); }
        [Moddable] public virtual int BasePierce { get => Resolve(); }
        [Moddable] public virtual int BaseBlast { get => Resolve(); }
        [Moddable] public virtual int BaseBurn { get => Resolve(); }
        [Moddable] public virtual int BaseEnergy { get => Resolve(); }

        [Moddable] public virtual int Impact { get => Resolve(); }
        [Moddable] public virtual int Pierce { get => Resolve(); }
        [Moddable] public virtual int Blast { get => Resolve(); }
        [Moddable] public virtual int Burn { get => Resolve(); }
        [Moddable] public virtual int Energy { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseBlast, x => x.BaseBlast),
                BaseModifier.Create(this, _basePierce, x => x.BasePierce),
                BaseModifier.Create(this, _baseImpact, x => x.BaseImpact),
                BaseModifier.Create(this, _baseBurn, x => x.BaseBurn),
                BaseModifier.Create(this, _baseEnergy, x => x.BaseEnergy),

                BaseModifier.Create(this, x => x.BaseBlast, x => x.Blast),
                BaseModifier.Create(this, x => x.BasePierce, x => x.Pierce),
                BaseModifier.Create(this, x => x.BaseImpact, x => x.Impact),
                BaseModifier.Create(this, x => x.BaseBurn, x => x.Burn),
                BaseModifier.Create(this, x => x.BaseEnergy, x => x.Energy)
            };
        }
    }

    public class CompositeResistances : Resistances
    {
        [JsonProperty] private Resistances[] _resistances { get; set; } = new Resistances[0];

        public CompositeResistances(params Resistances[] resistances)
        {
            _resistances = resistances;
        }

        public override int BaseImpact { get => _resistances.Sum(x => x.BaseImpact); }
        public override int BasePierce { get => _resistances.Sum(x => x.BasePierce); }
        public override int BaseBlast { get => _resistances.Sum(x => x.BaseBlast); }
        public override int BaseBurn { get => _resistances.Sum(x => x.BaseBurn); }
        public override int BaseEnergy { get => _resistances.Sum(x => x.BaseEnergy); }

        public override int Impact { get => _resistances.Sum(x => x.Impact); }
        public override int Pierce { get => _resistances.Sum(x => x.Pierce); }
        public override int Blast { get => _resistances.Sum(x => x.Blast); }
        public override int Burn { get => _resistances.Sum(x => x.Burn); }
        public override int Energy { get => _resistances.Sum(x => x.Energy); }
    }
}
