using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : Modifiable
    {
        public Damage() { }

        public Damage(string name)
        {
            Name = name;
        }

        public Damage(string name, string description, Dice baseDice)
        {
            Name = name;
            Description = description;
            BaseDice = baseDice;
        }

        [JsonProperty] public Dice BaseDice { get; private set; } = "d6";
        public Dice Dice => BaseDice + ModifierDice("Dice");
    }

    public class DamageSignature
    {
        [JsonProperty] public Damage Impact { get; protected set; } = new Damage(nameof(Impact));
        [JsonProperty] public Damage Pierce { get; protected set; } = new Damage(nameof(Pierce));
        [JsonProperty] public Damage Blast { get; protected set; } = new Damage(nameof(Blast));
        [JsonProperty] public Damage Burn { get; protected set; } = new Damage(nameof(Burn));
        [JsonProperty] public Damage Energy { get; protected set; } = new Damage(nameof(Energy));
    }
}
