using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact
    {
        public Artifact() 
        {
            Name = nameof(Artifact);
            Emissions = new EmissionSignature();
            Resistances = new Resistances();
            Health = new Health();
        }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public double BaseWeight { get; protected set; }
        public virtual double Weight => BaseWeight;

        [JsonProperty] public EmissionSignature Emissions { get; protected set; }
        [JsonProperty] public Resistances Resistances { get; protected set; }
        [JsonProperty] public Health Health { get; protected set; }

        [Ability("Destroy", "Destroy item")]
        public TurnAction Destroy()
        {
            return new TurnAction();
        }
    }
}
