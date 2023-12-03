using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

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
            this.Mod(() => BaseBlast, () => Blast).IsBase().Apply();
            this.Mod(() => BasePierce, () => Pierce).IsBase().Apply();
            this.Mod(() => BaseImpact, () => Impact).IsBase().Apply();
            this.Mod(() => BaseBurn, () => Burn).IsBase().Apply();
            this.Mod(() => BaseEnergy, () => Energy).IsBase().Apply();
        }
    }
}
