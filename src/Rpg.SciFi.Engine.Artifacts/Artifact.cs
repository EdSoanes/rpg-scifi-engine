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
            Nexus.Contexts.TryAdd(Id, this);

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
