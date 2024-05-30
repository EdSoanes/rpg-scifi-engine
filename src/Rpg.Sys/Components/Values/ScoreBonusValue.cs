using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.Sys.Components.Values
{
    public class ScoreBonusValue : RpgEntityComponent
    {
        [JsonProperty] public int Score { get; protected set; }
        [JsonProperty] public int Bonus { get; protected set; }

        [JsonConstructor] private ScoreBonusValue() { }

        public ScoreBonusValue(Guid entityId, string name, int score) 
            : base(entityId, name)
        {
            Score = score;
        }

        protected override void OnCreating()
        {
            this.AddMod(new Base(), x => x.Bonus, x => x.Score, () => CalculateStatBonus);
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
