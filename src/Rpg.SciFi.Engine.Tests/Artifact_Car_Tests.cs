using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
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
                    this.Mod("Noise", "20", () => Emissions.Sound.Value),
                    this.Mod("Heat", "15", () => Emissions.Heat.Value),
                    this.Mod("Electronics", "10", () => Emissions.Electromagnetic.Value)
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
            return new TurnAction(new TurnPoints(1, 1, 1), new State("Started"));
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
            Meta.Initialize(game);

            Assert.IsNotNull(Meta.MetaEntities);
        }

        [TestMethod]
        public void Artifact()
        {
            var car = new Car();
            var game = new Game();
            game.Environment.Contains.Add(car);
            Meta.Initialize(game);
            Assert.IsNotNull(Meta.MetaEntities);

            var meta = car.MetaData();
            Assert.IsNotNull(meta);

            meta = car.Emissions.VisibleLight.MetaData();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Electromagnetic.MetaData();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Heat.MetaData();
            Assert.IsNotNull(meta);

            meta = car.Emissions.Radiation.MetaData();
            Assert.IsNotNull(meta);

            meta =  car.Emissions.Sound.MetaData();
            Assert.IsNotNull(meta);

            meta = car.Movement.MetaData();
            Assert.IsNotNull(meta);

            foreach (var carPart in car.Parts)
            {
                meta = carPart.Health.MetaData();
                Assert.IsNotNull(meta);

                meta = carPart.Resistances.MetaData();
                Assert.IsNotNull(meta);
            }
        }
    }
}
