using Newtonsoft.Json;
using Rpg.Sys.Moddable;
using Rpg.Sys.Moddable.Modifiers;
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

        protected override void OnBuildGraph()
        {
            this.AddMod(x => x.Bonus, x => x.Score, () => CalculateStatBonus);
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
