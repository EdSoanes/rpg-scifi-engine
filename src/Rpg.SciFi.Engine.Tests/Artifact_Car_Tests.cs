using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Turns;

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
                    this.Mod("Noise", (car) => car.Emissions.Sound.Value, 20),
                    this.Mod("Heat", (car) => car.Emissions.Heat.Value, 15),
                    this.Mod("Electronics", (car) => car.Emissions.Electromagnetic.Value, 10)
                ));
        }

        [JsonProperty] public ArtifactPart[] Parts { get; protected set; }

        [JsonProperty] public Movement Movement { get; private set; } = new Movement(
            baseSpeed: 180, 
            baseAcceleration: 10, 
            baseDeceleration: 30, 
            baseManeuverability: 5);

        [JsonProperty] public States States { get; protected set; }


        [JsonProperty] public Abilities Abilities { get; protected set; } = new Abilities();
        [JsonProperty] public EmissionSignature Emissions { get; protected set; } = new EmissionSignature();

        [Ability("Start", "Start the car")]
        public TurnAction Start()
        {
            return new TurnAction();
        }

    }

    [TestClass]
    public class ArtifactTests
    {
        [TestMethod]
        public void MetaDiscovery_DiscoverCar()
        {
            var game = new Game();
            game.Environment.Contains.Add(new Car());
            MetaDiscovery.Initialize(game);

            Assert.IsNotNull(MetaDiscovery.MetaEntities);
        }

        [TestMethod]
        public void Artifact()
        {
            var car = new Car();
            var game = new Game();
            game.Environment.Contains.Add(car);
            MetaDiscovery.Initialize(game);
            Assert.IsNotNull(MetaDiscovery.MetaEntities);

            var meta = MetaDiscovery.Find(car.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Emissions.VisibleLight.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Emissions.Electromagnetic.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Emissions.Heat.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Emissions.Radiation.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Emissions.Sound.Id);
            Assert.IsNotNull(meta);

            meta = MetaDiscovery.Find(car.Movement.Id);
            Assert.IsNotNull(meta);

            foreach (var carPart in car.Parts)
            {
                meta = MetaDiscovery.Find(carPart.Health.Id);
                Assert.IsNotNull(meta);

                meta = MetaDiscovery.Find(carPart.Resistances.Id);
                Assert.IsNotNull(meta);
            }
        }
    }
}
