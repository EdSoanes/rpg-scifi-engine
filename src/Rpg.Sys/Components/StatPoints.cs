﻿using Newtonsoft.Json;
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

        public StatPoints(string entityId, string name, StatPointsTemplate template)
            : base(entityId, name)
        {
            Strength = new ScoreBonusValue(entityId, nameof(Strength), template.Strength);
            Intelligence = new ScoreBonusValue(entityId, nameof(Intelligence), template.Intelligence);
            Wisdom = new ScoreBonusValue(entityId, nameof(Wisdom), template.Wisdom);
            Dexterity = new ScoreBonusValue(entityId, nameof(Dexterity), template.Dexterity);
            Constitution = new ScoreBonusValue(entityId, nameof(Constitution), template.Constitution);
            Charisma = new ScoreBonusValue(entityId, nameof(Charisma), template.Charisma);
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Strength.OnBeforeTime(graph, entity);
            Intelligence.OnBeforeTime(graph, entity);
            Wisdom.OnBeforeTime(graph, entity);
            Dexterity.OnBeforeTime(graph, entity);
            Constitution.OnBeforeTime(graph, entity);
            Charisma.OnBeforeTime(graph, entity);
        }
    }
}
