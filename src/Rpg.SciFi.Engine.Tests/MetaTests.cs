using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Archetypes.Templates;
using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class MetaTests
    {
        private EntityGraph _graph;
        private Game _game = new Game();
        private Gun _gun;
        private Character _target;

        [TestInitialize]
        public void Initialize()
        {
            var rifle = new Rifle
            {
                Attack = 2
            };

            _gun = new Gun(rifle) { Name = "Blaster" };

            var player = new Character("The Player");
            player.GetContainer(Container.RightHand)!.Add(_gun);

            _target = new Character("The Target");

            _game.Character = player;
            _game.Environment.GetContainer(Container.Environment)!.Add(_target);

            _graph = new EntityGraph();
            _graph.Initialize(_game);
        }

        [TestMethod]
        public void Game_Describe()
        {
            var desc = _graph.Describe();
            Assert.IsNotNull(desc);
        }
    }
}
