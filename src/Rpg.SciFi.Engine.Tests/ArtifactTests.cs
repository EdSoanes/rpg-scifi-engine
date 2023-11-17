using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Turns;

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
            Assert.IsNotNull(paths);
        }
    }
}
