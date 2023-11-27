using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : Entity
    {
        public Damage(Dice baseImpact, Dice basePierce, Dice baseBlast, Dice baseBurn, Dice baseEnergy)
        {
            BaseImpact = baseImpact;
            BasePierce = basePierce;
            BaseBlast = baseBlast;
            BaseBurn = baseBurn;
            BaseEnergy = baseEnergy;
        }

        [JsonProperty] public Dice BaseImpact { get; protected set; }
        [JsonProperty] public Dice BasePierce { get; protected set; }
        [JsonProperty] public Dice BaseBlast { get; protected set; }
        [JsonProperty] public Dice BaseBurn { get; protected set; }
        [JsonProperty] public Dice BaseEnergy { get; protected set; }

        [Moddable] public Dice Impact { get => this.Evaluate(nameof(Impact)); }
        [Moddable] public Dice Pierce { get => this.Evaluate(nameof(Pierce)); }
        [Moddable] public Dice Blast { get => this.Evaluate(nameof(Blast)); }
        [Moddable] public Dice Burn { get => this.Evaluate(nameof(Burn)); }
        [Moddable] public Dice Energy { get => this.Evaluate(nameof(Energy)); }

        [Setup]
        public void Setup()
        {
            this.AddBaseMod(x => x.BaseBlast, x => x.Blast);
            this.AddBaseMod(x => x.BasePierce, x => x.Pierce);
            this.AddBaseMod(x => x.BaseImpact, x => x.Impact);
            this.AddBaseMod(x => x.BaseBurn,x => x.Burn);
            this.AddBaseMod(x => x.BaseEnergy, x => x.Energy);
        }
    }
}
