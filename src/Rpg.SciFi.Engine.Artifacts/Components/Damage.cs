using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : Entity
    {
        private readonly Dice _baseImpact;
        private readonly Dice _basePierce;
        private readonly Dice _baseBlast;
        private readonly Dice _baseBurn;
        private readonly Dice _baseEnergy;

        [JsonConstructor] public Damage() { }

        public Damage(Dice baseImpact, Dice basePierce, Dice baseBlast, Dice baseBurn, Dice baseEnergy)
        {
            _baseImpact = baseImpact;
            _basePierce = basePierce;
            _baseBlast = baseBlast;
            _baseBurn = baseBurn;
            _baseEnergy = baseEnergy;
        }

        [Moddable] public Dice BaseImpact { get => this.Evaluate(nameof(BaseImpact)); }
        [Moddable] public Dice BasePierce { get => this.Evaluate(nameof(BasePierce)); }
        [Moddable] public Dice BaseBlast { get => this.Evaluate(nameof(BaseBlast)); }
        [Moddable] public Dice BaseBurn { get => this.Evaluate(nameof(BaseBurn)); }
        [Moddable] public Dice BaseEnergy { get => this.Evaluate(nameof(BaseEnergy)); }

        [Moddable] public Dice Impact { get => this.Evaluate(nameof(Impact)); }
        [Moddable] public Dice Pierce { get => this.Evaluate(nameof(Pierce)); }
        [Moddable] public Dice Blast { get => this.Evaluate(nameof(Blast)); }
        [Moddable] public Dice Burn { get => this.Evaluate(nameof(Burn)); }
        [Moddable] public Dice Energy { get => this.Evaluate(nameof(Energy)); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                this.Mod(nameof(BaseBlast), _baseBlast, (x) => x.BaseBlast),
                this.Mod(nameof(BasePierce), _basePierce, (x) => x.BasePierce),
                this.Mod(nameof(BaseImpact), _baseImpact, (x) => x.BaseImpact),
                this.Mod(nameof(BaseBurn), _baseBurn, (x) => x.BaseBurn),
                this.Mod(nameof(BaseEnergy), _baseEnergy, (x) => x.BaseEnergy),

                this.Mod((x) => x.BaseBlast, (x) => x.Blast),
                this.Mod((x) => x.BasePierce, (x) => x.Pierce),
                this.Mod((x) => x.BaseImpact, (x) => x.Impact),
                this.Mod((x) => x.BaseBurn, (x) => x.Burn),
                this.Mod((x) => x.BaseEnergy, (x) => x.Energy)
            };
        }
    }
}
