using Newtonsoft.Json;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class ScoreBonusValue : RpgEntityComponent
    {
        [JsonProperty] 
        [MetaPropUI(Min = 3, Max = 18)]
        public int Score { get; protected set; }

        [MetaPropUI(Ignore = true)]
        [JsonProperty] public int Bonus { get; protected set; }

        [JsonConstructor] private ScoreBonusValue() { }

        public ScoreBonusValue(string entityId, string name, int score) 
            : base(entityId, name)
        {
            Score = score;
        }

        protected override void OnCreating()
        {
            this.BaseMod(x => x.Bonus, x => x.Score, () => CalculateStatBonus);
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
