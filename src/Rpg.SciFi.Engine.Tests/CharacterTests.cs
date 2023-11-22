using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class CharacterTests
    {
        [TestMethod]
        public void Character_Init_DamageBuff()
        {
            var game = new Game();
            game.Character = new Character();
            MetaDiscovery.Initialize(game);

            Assert.AreEqual("1d6", game.Character.Damage.BaseImpact.ToString());
            Assert.AreEqual("1d6 + 4", game.Character.Damage.Impact.ToString());
        }
    }
}
