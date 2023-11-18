using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Components;
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
        }

        [JsonProperty] public ArtifactPart[] Parts { get; protected set; }

        [JsonProperty] public Movement Movement { get; private set; } = new Movement(
            baseSpeed: 180, 
            baseAcceleration: 10, 
            baseDeceleration: 30, 
            baseManeuverability: 5);

        [JsonProperty] public States States { get; protected set; } = new States(
                new State("Activated",
                    new Modifier("Noise", "Emissions.Sound", "20"),
                    new Modifier("Heat", "Emissions.Heat", "15"),
                    new Modifier("Electronics", "Emissions.Electromagnetic", "10"))
                );


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
        public void Artifact()
        {
            var car = new Car();
            Nexus.BuildActionableLists();
            var paths = Nexus.GetPropertyPaths(car);

            Assert.IsNotNull(Nexus.Actions);
            Assert.IsNotNull(Nexus.Props);

            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Id, out var carId));
            Assert.IsFalse(Nexus.Contexts.TryGetValue(Guid.NewGuid(), out var testId));

            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Emissions.VisibleLight.Id, out var visibleLightId));
            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Emissions.Electromagnetic.Id, out var electromagneticId));
            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Emissions.Heat.Id, out var heatId));
            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Emissions.Radiation.Id, out var radiationId));
            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Emissions.Sound.Id, out var soundId));
            Assert.IsTrue(Nexus.Contexts.TryGetValue(car.Movement.Id, out var movementId));

            foreach (var carPart in car.Parts)
            {
                Assert.IsTrue(Nexus.Contexts.TryGetValue(carPart.Health.Id, out var carPartHealthId));
                Assert.IsTrue(Nexus.Contexts.TryGetValue(carPart.Resistances.Id, out var carPartResistancesId));
            }

            
            Assert.IsNotNull(paths);
        }
    }
}
