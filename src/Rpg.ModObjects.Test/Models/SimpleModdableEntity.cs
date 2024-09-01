using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Models
{
    public class SimpleModdableEntity : RpgEntity
    {
        public int Score { get; protected set; }
        public int Bonus { get; protected set; }

        public SimpleModdableEntity(int score, int bonus)
        {
            Score = score;
            Bonus = bonus;
        }


        public override void OnTimeBegins()
        {
            base.OnTimeBegins();

            this.AddMod(new Base(), x => x.Score, x => x.Bonus);
        }
    }
}
