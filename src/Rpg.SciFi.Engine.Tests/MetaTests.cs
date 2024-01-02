using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaTests
    {
        private EntityManager<Game> _meta;
        private Game _game = new Game();
        private Gun _gun;
        private Character _target;

        [TestInitialize]
        public void Initialize()
        {
            _gun = new Gun(10, 2) { Name = "Blaster" };

            var player = new Character("The Player");
            player.GetContainer(Container.RightHand)!.Add(_gun);

            _target = new Character("The Target");

            _game.Character = player;
            _game.Environment.GetContainer(Container.Environment)!.Add(_target);

            _meta = new EntityManager<Game>();
            _meta.Initialize(_game);
        }

        [TestMethod]
        public void Game_Describe()
        {
            var desc = _meta.Describe();
            Assert.IsNotNull(desc);
        }
    }
}
