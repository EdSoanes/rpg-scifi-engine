using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class ScoreBonusValue : RpgEntityComponent
    {
        [JsonProperty] public int Score { get; protected set; }
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
