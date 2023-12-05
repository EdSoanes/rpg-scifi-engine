using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class CharacterTests
    {
        [TestMethod]
        public void Rules_CalculateStatBonus()
        {
            Assert.AreEqual<int>(0, Rules.CalculateStatBonus(10).Roll());
            Assert.AreEqual<int>(4, Rules.CalculateStatBonus(18).Roll());
            Assert.AreEqual<int>(-4, Rules.CalculateStatBonus(3).Roll());

        }

        [TestMethod]

        public void Character_Init_DamageBuff()
        {
            var game = new Game();
            game.Character = new Character();
            Meta.Initialize(game);

            Assert.AreEqual<string>("1d6", game.Character.Damage.BaseImpact);
            Assert.AreEqual<string>("1d6 + 4", game.Character.Damage.Impact);
        }

        [TestMethod]
        public void Character_Gun_Fire()
        {
            var game = new Game();
            var player = new Character();
            var target = new Character();
            var gun = new Gun(10, 2);

            game.Character = player;
            game.Character.Equipment.Add(gun);
            game.Environment.Contains.Add(target);

            Meta.Initialize(game);

            var action = gun.Fire(game.Character, target, 3);
            Assert.AreEqual<string>("1d20 - 1", action.DiceRoll);

            var description = action.Describe(nameof(TurnAction.DiceRoll));

            Assert.AreEqual<string>("1d6", game.Character.Damage.BaseImpact);
            Assert.AreEqual<string>("1d6 + 4", game.Character.Damage.Impact);
        }
    }
}
