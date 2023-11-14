using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact
    {
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = string.Empty;
        [JsonProperty] public string Description { get; protected set; } = string.Empty;
        [JsonProperty] public double BaseWeight { get; protected set; } = 0.0;
        public double Weight => BaseWeight + Contents.Sum(x => x.Weight);

        [JsonProperty] public States States { get; protected set; } = new States();
        [JsonProperty] public Abilities Abilities { get; protected set; } = new Abilities();
        [JsonProperty] public EmissionSignature Emissions { get; protected set; } = new EmissionSignature();
        [JsonProperty] public ResistanceSignature Resistances { get; protected set; } = new ResistanceSignature();

        [JsonProperty] public Artifact[] Contents { get; protected set; } = new Artifact[0];
        [JsonProperty] public int MaxContents { get; protected set; } = 0;
    }
}
