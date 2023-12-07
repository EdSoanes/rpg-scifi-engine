using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Character : Artifact
    {
        public Character()
        {
            Emissions = new EmissionSignature(null, new Emission("Heat", 36));
            Health = new Health(10, 10);
            TurnPoints = new TurnPoints(8, 8, 8);
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

        [JsonProperty] public TurnPoints TurnPoints { get; private set; }
        [JsonProperty] public StatPoints Stats { get; private set; }
        [JsonProperty] public Contains Equipment { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }

        [Setup]
        public override void Setup()
        {
            base.Setup();
            this.Mod((x) => x.Stats.StrengthBonus, (x) => x.Damage.Impact).IsBase().Apply();
            this.Mod((x) => x.Stats.StrengthBonus, (x) => x.TurnPoints.Exertion).IsBase().Apply();
            this.Mod((x) => x.Stats.DexterityBonus, (x) => x.TurnPoints.Action).IsBase().Apply();
            this.Mod((x) => x.Stats.IntelligenceBonus, (x) => x.TurnPoints.Focus).IsBase().Apply();
        }
    }
}
