using Newtonsoft.Json;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class ScoreBonusValue : ModdableObject
    {
        [JsonProperty] public int Score {  get; protected set; }
        [JsonProperty] public int Bonus { get; protected set; }

        [JsonConstructor] private ScoreBonusValue() { }

        public ScoreBonusValue(int score) 
        {
            Score = score;
        }

        public override Modifier[] OnSetup()
        {
            var mods = new List<Modifier>(base.OnSetup())
            {
                BaseModifier.Create(this, x => x.Score, x => x.Bonus, () => CalculateStatBonus)
            };

            return mods.ToArray();
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }
}
