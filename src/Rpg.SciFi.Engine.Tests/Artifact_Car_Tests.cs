using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    public class Car : Artifact
    {
        [JsonConstructor]
        private Car(string name) 
        {
            Name = name;
        }

        public Car() 
        { 
            Name = nameof(Car);
            Parts = new ArtifactPart[]
            {
                new ArtifactPart("Chassis", 50),
                new ArtifactPart("Engine", 30)
            };

            Health = new CompositeHealth(Parts.Select(x => x.Health).ToArray());
            Resistances = new CompositeResistances(Parts.Select(x => x.Resistances).ToArray());
            States = new States(
                new State("Activated",
                    BaseModifier.Create(this, "20", (x) => x.Emissions.Sound.Value),
                    BaseModifier.Create(this, "15", (x) => x.Emissions.Heat.Value),
                    BaseModifier.Create(this, "10", (x) => x.Emissions.Electromagnetic.Value)
                ));
        }

        [JsonProperty] public ArtifactPart[] Parts { get; protected set; }

        [JsonProperty] public Movement Movement { get; private set; } = new Movement(
            baseSpeed: 180, 
            baseAcceleration: 10, 
            baseDeceleration: 30, 
            baseManeuverability: 5);

        [JsonProperty] public Abilities Abilities { get; protected set; } = new Abilities();

        [Ability]
        public TurnAction Start()
        {
            return new TurnAction(Graph!, nameof(Start), 1, 1, 1);
        }
    }

    [TestClass]
    public class ArtifactTests
    {
        private EntityGraph _graph;
        private Car _car;

        [TestInitialize]
        public void Initialize()
        {
            _car = new Car();

            var game = new Game();
            game.Environment.GetContainer(Container.Environment)!.Add(_car);

            _graph = new EntityGraph();
            _graph.Initialize(game);
        }

        [TestMethod]
        public void Artifact()
        {
            var meta = _car.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.VisibleLight.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Electromagnetic.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Heat.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Radiation.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Sound.Meta;
            Assert.IsNotNull(meta);

            meta = _car.Movement.Meta;
            Assert.IsNotNull(meta);

            foreach (var carPart in _car.Parts)
            {
                meta = carPart.Health.Meta;
                Assert.IsNotNull(meta);

                meta = carPart.Resistances.Meta;
                Assert.IsNotNull(meta);
            }
        }
    }
}
