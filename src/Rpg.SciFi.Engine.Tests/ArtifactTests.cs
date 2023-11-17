using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Tests
{
    public class Car : Artifact
    {
        public Car() 
        { 
            Name = nameof(Car);
            Description = "This is a car";
            Parts = new ArtifactPart[]
            {
                new ArtifactPart("Chassis", "Car chassis", 50),
                new ArtifactPart("Engine", "Turbo Engine", 30)
            };
        }

        [JsonProperty] public Movement Movement { get; private set; } = new Movement(
            baseSpeed: 180, 
            baseAcceleration: 10, 
            baseDeceleration: 30, 
            baseManeuverability: 5);

        [JsonProperty] public States States { get; protected set; } = new States(
                new State("Activated", "Car engine running",
                    new Modifier("Noise", "Emissions.Sound.Value", "20"),
                    new Modifier("Heat", "Emissions.Heat.Value", "15"),
                    new Modifier("Electronics", "Emissions.Electromagnetic", "10"))
                );


        [JsonProperty] public Abilities Abilities { get; protected set; } = new Abilities();
        [JsonProperty] public EmissionSignature Emissions { get; protected set; } = new EmissionSignature();

    }
    [TestClass]
    public class ArtifactTests
    {
        [TestMethod]
        public void Artifact()
        {
            var car = new Car();
            var actions = Nexus.GetActions(car);
            Assert.IsNotNull(actions);
        }
    }
}
