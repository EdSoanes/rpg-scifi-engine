using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact
    {
        public Artifact() 
        { 
            Health = new CompositeHealth(Parts.Select(x => x.Health).ToArray());
        }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = string.Empty;
        [JsonProperty] public string Description { get; protected set; } = string.Empty;
        [JsonProperty] public double BaseWeight { get; protected set; } = 0.0;
        public double Weight => BaseWeight + Contents.Sum(x => x.Weight);

        [JsonProperty] public Resistances Resistances { get; protected set; } = new Resistances();
        [JsonProperty] public Health Health {  get; protected set; }

        [JsonProperty] public Artifact[] Contents { get; protected set; } = new Artifact[0];
        [JsonProperty] public int MaxContents { get; protected set; } = 0;
        [JsonProperty] public ArtifactPart[] Parts { get; protected set; } = new ArtifactPart[0];

        [Ability("Destroy", "Destroy item")]
        public TurnAction Destroy()
        {
            return new TurnAction();
        }
    }
}
