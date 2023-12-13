using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaTests
    {
        private Meta<Game> _meta;
        private Game _game = new Game();
        private Gun _gun;
        private Character _target;

        [TestInitialize]
        public void Initialize()
        {
            _gun = new Gun(10, 2) { Name = "Blaster" };

            var player = new Character("The Player");
            player.Equipment.Add(_gun);

            _target = new Character("The Target");

            _game.Character = player;
            _game.Environment.Contains.Add(_target);

            _meta = new Meta<Game>();
            _meta.Initialize(_game);
        }

        [TestMethod]
        public void Game_Describe()
        {
            var desc = _meta.Describe();
            Assert.IsNotNull(desc);
        }

        [TestMethod]
        public void Game_Describe_Gun_Mods()
        {
            foreach (var prop in _gun.MetaData.ModdableProperties)
            {
                var desc = _gun.Describe(prop);
                Assert.IsNotNull(desc);
            } 
        }

        [TestMethod]
        public void Game_Describe_Target_Mods()
        {
            foreach (var prop in _target.MetaData.ModdableProperties)
            {
                var desc = _target.Describe(prop);
                Assert.IsNotNull(desc);
            }
        }
    }
}
