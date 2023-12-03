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

        [JsonProperty] public TurnPoints TurnPoints { get; private set; }
        [JsonProperty] public StatPoints Stats { get; private set; }
        [JsonProperty] public Contains Equipment { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }

        [Setup]
        public void Setup()
        {
            this.Mod(() => Stats.StrengthBonus, () => Damage.Impact).IsBase().Apply();
            this.Mod(() => Stats.StrengthBonus, () => TurnPoints.Exertion).IsBase().Apply();
            this.Mod(() => Stats.DexterityBonus, () => TurnPoints.Action).IsBase().Apply();
            this.Mod(() => Stats.IntelligenceBonus, () => TurnPoints.Focus).IsBase().Apply();
        }
    }
}
