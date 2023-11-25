using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;
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
                    this.Modifies("Noise", 20, (car) => car.Emissions.Sound.Value),
                    this.Modifies("Heat", 15, (car) => car.Emissions.Heat.Value),
                    this.Modifies("Electronics", 10, (car) => car.Emissions.Electromagnetic.Value)
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
            MetaEngine.Initialize(game);

            Assert.IsNotNull(MetaEngine.MetaEntities);
        }

        [TestMethod]
        public void Artifact()
        {
            var car = new Car();
            var game = new Game();
            game.Environment.Contains.Add(car);
            MetaEngine.Initialize(game);
            Assert.IsNotNull(MetaEngine.MetaEntities);

            var meta = car.Meta();
            Assert.IsNotNull(meta);

            meta = car.Emissions.VisibleLight.Meta();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Electromagnetic.Meta();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Heat.Meta();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Radiation.Meta();
            Assert.IsNotNull(meta);

            meta =  car.Emissions.Sound.Meta();
            Assert.IsNotNull(meta);

            meta = car.Movement.Meta();
            Assert.IsNotNull(meta);

            foreach (var carPart in car.Parts)
            {
                meta = carPart.Health.Meta();
                Assert.IsNotNull(meta);

                meta = carPart.Resistances.Meta();
                Assert.IsNotNull(meta);
            }
        }
    }
}
