using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Character : Artifact
    {
        public Character()
        {
            Emissions = new EmissionSignature(null, new Emission("Heat", 36));
            Health = new Health(10, 10);
            Stats = new StatPoints
            {
                BaseStrength = 18,
                BaseIntelligence = 14,
                BaseDexterity = 5
            };

            Stats.AddModifier(this.Modifies(nameof(Stats.StrengthBonus), (c) => c.Damage.Impact, (c) => c.Stats.StrengthBonus));
        }

        [JsonProperty] public StatPoints Stats { get; private set; }

        [JsonProperty] public Contains Equipment { get; private set; } = new Contains();

        [JsonProperty] public Damage Damage { get; private set; } = new Damage("d6", "0", "0", "0", "0");
    }
}
