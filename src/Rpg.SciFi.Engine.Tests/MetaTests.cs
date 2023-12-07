using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaTests
    {
        [TestMethod]
        public void Game_Describe()
        {
            var game = new Game();
            var player = new Character("The Player");
            var target = new Character("The Target");
            var gun = new Gun(10, 2);

            game.Character = player;
            game.Character.Equipment.Add(gun);
            game.Environment.Contains.Add(target);

            Meta.Initialize(game);

            var desc = Meta.Describe();
            Assert.IsNotNull(desc);
        }

        [TestMethod]
        public void Game_Describe_Gun_Mods()
        {
            var game = new Game();
            var player = new Character("The Player");
            var target = new Character("The Target");
            var gun = new Gun(10, 2);

            game.Character = player;
            game.Character.Equipment.Add(gun);
            game.Environment.Contains.Add(target);

            Meta.Initialize(game);

            foreach (var prop in gun.MetaData().Mods.ModdableProperties)
            {
                var desc = gun.Describe(prop);
                Assert.IsNotNull(desc);
            } 
        }

        [TestMethod]
        public void Game_Describe_Target_Mods()
        {
            var game = new Game();
            var player = new Character("The Player");
            var target = new Character("The Target");
            var gun = new Gun(10, 2);

            game.Character = player;
            game.Character.Equipment.Add(gun);
            game.Environment.Contains.Add(target);

            Meta.Initialize(game);

            foreach (var prop in target.MetaData().Mods.ModdableProperties)
            {
                var desc = target.Describe(prop);
                Assert.IsNotNull(desc);
            }
        }
    }
}
