using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;

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
            game.Character.Setup();

            Assert.AreEqual<string>("1d6", game.Character.Damage.BaseImpact);
            Assert.AreEqual<string>("1d6 + 4", game.Character.Damage.Impact);
        }
    }
}
