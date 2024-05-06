using Newtonsoft.Json;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class ScoreBonusValue : ModObject
    {
        [JsonProperty] public int Score { get; protected set; }
        [JsonProperty] public int Bonus { get; protected set; }

        [JsonConstructor] private ScoreBonusValue() { }

        public ScoreBonusValue(string name, int score) 
        {
            Name = name;
            Score = score;
        }

        protected override void OnInitialize()
        {
            PropStore.Init(this, BaseModifier.Create(this, x => x.Score, x => x.Bonus, () => CalculateStatBonus));
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
