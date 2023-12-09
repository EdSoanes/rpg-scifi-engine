using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Character : Artifact
    {
        public Character()
        {
            Emissions = new EmissionSignature(null, new Emission("Heat", 36));
            Health = new Health(10, 10);
            Turns = new TurnPoints(8, 8, 8);
            Damage = new Damage("d6", "0", "0", "0", "0");
            Equipment = new Contains();

            Stats = new StatPoints
            {
                BaseStrength = 18,
                BaseIntelligence = 14,
                BaseDexterity = 5
            };
        }

        public Character(string name)
            : this()
        {
            Name = name;
        }

        [JsonProperty] public TurnPoints Turns { get; private set; }
        [JsonProperty] public StatPoints Stats { get; private set; }
        [JsonProperty] public Contains Equipment { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }

        [Setup]
        public override void Setup()
        {
            base.Setup();
            Meta.Mods.Add(this.Mod((x) => x.Stats.StrengthBonus, (x) => x.Damage.Impact).IsBase());
            Meta.Mods.Add(this.Mod((x) => x.Stats.StrengthBonus, (x) => x.Turns.Exertion).IsBase());
            Meta.Mods.Add(this.Mod((x) => x.Stats.DexterityBonus, (x) => x.Turns.Action).IsBase());
            Meta.Mods.Add(this.Mod((x) => x.Stats.IntelligenceBonus, (x) => x.Turns.Focus).IsBase());
        }
    }
}
