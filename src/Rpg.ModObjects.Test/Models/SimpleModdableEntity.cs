using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Tests.Models
{
    public class SimpleModdableEntity : RpgObject
    {
        public int Score { get; protected set; }
        public int Bonus { get; protected set; }

        public SimpleModdableEntity(int score, int bonus)
        {
            Score = score;
            Bonus = bonus;
        }

        protected override void OnCreate()
        {
            this.AddMod(new Base(), x => x.Score, x => x.Bonus);
        }
    }
}
