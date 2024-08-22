using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;
using System.Runtime.ConstrainedExecution;

namespace Rpg.Sys.Components
{
    public class StatPoints : RpgComponent
    {
        [JsonProperty] public ScoreBonusValue Strength { get; private set; }
        [JsonProperty] public ScoreBonusValue Intelligence { get; private set; }
        [JsonProperty] public ScoreBonusValue Wisdom { get; private set; }
        [JsonProperty] public ScoreBonusValue Dexterity { get; private set; }
        [JsonProperty] public ScoreBonusValue Constitution { get; private set; }
        [JsonProperty] public ScoreBonusValue Charisma { get; private set; }

        [JsonConstructor] private StatPoints() { }

        public StatPoints(string name, StatPointsTemplate template)
            : base(name)
        {
            Strength = new ScoreBonusValue(nameof(Strength), template.Strength);
            Intelligence = new ScoreBonusValue(nameof(Intelligence), template.Intelligence);
            Wisdom = new ScoreBonusValue(nameof(Wisdom), template.Wisdom);
            Dexterity = new ScoreBonusValue(nameof(Dexterity), template.Dexterity);
            Constitution = new ScoreBonusValue(nameof(Constitution), template.Constitution);
            Charisma = new ScoreBonusValue(nameof(Charisma), template.Charisma);
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Strength.OnCreating(graph, entity);
            Intelligence.OnCreating(graph, entity);
            Wisdom.OnCreating(graph, entity);
            Dexterity.OnCreating(graph, entity);
            Constitution.OnCreating(graph, entity);
            Charisma.OnCreating(graph, entity);
        }
    }
}
