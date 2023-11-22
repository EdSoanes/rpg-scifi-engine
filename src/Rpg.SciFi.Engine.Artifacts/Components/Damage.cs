using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : Modifiable
    {
        public Damage() 
        {
            Name = nameof(Damage);
        }

        public Damage(Dice baseImpact, Dice basePierce, Dice baseBlast, Dice baseBurn, Dice baseEnergy)
            : this()
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

        [Moddable] public Dice Impact { get => BaseImpact + ModifierDice(nameof(Impact)); }
        [Moddable] public Dice Pierce { get => BasePierce + ModifierDice(nameof(Pierce)); }
        [Moddable] public Dice Blast { get => BaseBlast + ModifierDice(nameof(Blast)); }
        [Moddable] public Dice Burn { get => BaseBurn + ModifierDice(nameof(Burn)); }
        [Moddable] public Dice Energy { get => BaseEnergy + ModifierDice(nameof(Energy)); }
    }
}
