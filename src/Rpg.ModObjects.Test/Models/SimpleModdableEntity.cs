using Rpg.ModObjects.Mods;
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

        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.Score, x => x.Bonus);
        }
    }
}
