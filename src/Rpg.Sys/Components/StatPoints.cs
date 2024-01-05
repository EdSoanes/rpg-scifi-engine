using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class StatPoints
    {
        [JsonProperty] public ScoreBonusValue Strength { get; private set; }
        [JsonProperty] public ScoreBonusValue Intelligence { get; private set; }
        [JsonProperty] public ScoreBonusValue Wisdom { get; private set; }
        [JsonProperty] public ScoreBonusValue Dexterity { get; private set; }
        [JsonProperty] public ScoreBonusValue Constitution { get; private set; }
        [JsonProperty] public ScoreBonusValue Charisma { get; private set; }

        [JsonConstructor] private StatPoints() { }

        public StatPoints(StatPointsTemplate template)
        {
            Strength = new ScoreBonusValue(template.Strength);
            Intelligence = new ScoreBonusValue(template.Intelligence);
            Wisdom = new ScoreBonusValue(template.Wisdom);
            Dexterity = new ScoreBonusValue(template.Dexterity);
            Constitution = new ScoreBonusValue(template.Constitution);
            Charisma = new ScoreBonusValue(template.Charisma);
        }
    }
}
