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
        public ArtifactPart() 
        {
            Name = nameof(ArtifactPart);
        }

        public ArtifactPart(string name, int physical)
        {
            Name = name;
            Health = new Health(physical, -1);
        }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(ArtifactPart);
        [JsonProperty] public Health Health { get; protected set; } = new Health();
        [JsonProperty] public Resistances Resistances { get; protected set; } = new Resistances();
    }
}
