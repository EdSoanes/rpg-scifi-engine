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
            Strength = new ScoreBonusValue(nameof(Strength), template.Strength);
            Intelligence = new ScoreBonusValue(nameof(Intelligence), template.Intelligence);
            Wisdom = new ScoreBonusValue(nameof(Wisdom), template.Wisdom);
            Dexterity = new ScoreBonusValue(nameof(Dexterity), template.Dexterity);
            Constitution = new ScoreBonusValue(nameof(Constitution), template.Constitution);
            Charisma = new ScoreBonusValue(nameof(Charisma), template.Charisma);
        }
    }
}
