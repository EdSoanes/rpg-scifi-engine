using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class ArtifactPart
    {
        public ArtifactPart() { }
        public ArtifactPart(string name, string? description, int physical)
        {
            Name = name;
            Description = description;
            Health = new HealthPoints(physical, Score.NotApplicable);
        }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(ArtifactPart);
        [JsonProperty] public string? Description { get; protected set; } = string.Empty;

        [JsonProperty] public HealthPoints Health { get; protected set; } = new HealthPoints();

        [JsonProperty] public ResistanceSignature Resistances { get; protected set; } = new ResistanceSignature();
    }

    public class ArtifactPartScore : Score
    {
        [JsonProperty] protected ArtifactPart[] Parts;

        public ArtifactPartScore(ArtifactPart[] parts) 
        {
            Parts = parts;
            BaseValue = parts.Sum(x => x.Health.Physical.BaseValue);
        }

        public override int Value => BaseValue + Parts.Sum(x => x.Health.Physical.Value);
    }
}
