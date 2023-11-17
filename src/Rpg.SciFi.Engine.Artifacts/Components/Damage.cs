using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : Modifiable
    {
        public Damage() { }
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

        [Modifiable("Impact", "Impact")] public Dice Impact { get => BaseImpact + ModifierDice(nameof(Impact)); }
        [Modifiable("Pierce", "Pierce")] public Dice Pierce { get => BasePierce + ModifierDice(nameof(Pierce)); }
        [Modifiable("Blast", "Blast")] public Dice Blast { get => BaseBlast + ModifierDice(nameof(Blast)); }
        [Modifiable("Burn", "Burn")] public Dice Burn { get => BaseBurn + ModifierDice(nameof(Burn)); }
        [Modifiable("Energy", "Energy")] public Dice Energy { get => BaseEnergy + ModifierDice(nameof(Energy)); }
    }
}
