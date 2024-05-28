using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Tests.Models
{
    public class SimpleModdableEntity : ModObject
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
            this.AddMod<BaseBehavior, SimpleModdableEntity, int, int>(x => x.Score, x => x.Bonus);
        }
    }
}
