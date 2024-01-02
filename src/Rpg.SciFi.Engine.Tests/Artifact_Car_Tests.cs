using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
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
            return new TurnAction(ModStore!, Evaluator!, nameof(Start), 1, 1, 1);
        }
    }

    [TestClass]
    public class ArtifactTests
    {
        private EntityManager<Game> _meta;
        private Car _car;

        [TestInitialize]
        public void Initialize()
        {
            _car = new Car();

            var game = new Game();
            game.Environment.GetContainer(Container.Environment)!.Add(_car);

            _meta = new EntityManager<Game>();
            _meta.Initialize(game);
        }

        [TestMethod]
        public void Artifact()
        {
            var meta = _car.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.VisibleLight.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Electromagnetic.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Heat.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Radiation.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Emissions.Sound.MetaData;
            Assert.IsNotNull(meta);

            meta = _car.Movement.MetaData;
            Assert.IsNotNull(meta);

            foreach (var carPart in _car.Parts)
            {
                meta = carPart.Health.MetaData;
                Assert.IsNotNull(meta);

                meta = carPart.Resistances.MetaData;
                Assert.IsNotNull(meta);
            }
        }
    }
}
