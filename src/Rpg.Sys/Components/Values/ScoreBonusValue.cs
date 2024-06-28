using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;
using Rpg.Sys.Attributes;

namespace Rpg.Sys.Components.Values
{
    public class ScoreBonusValue : RpgComponent
    {
        [JsonProperty]
        [ScoreUI()]
        public int Score { get; protected set; }

        [JsonProperty]
        [IntegerUI(Ignore = true)]
        public int Bonus { get; protected set; }

        [JsonConstructor] private ScoreBonusValue() { }

        public ScoreBonusValue(string entityId, string name, int score) 
            : base(entityId, name)
        {
            Score = score;
        }

        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.Bonus, x => x.Score, () => CalculateStatBonus);
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
