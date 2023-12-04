using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Artifact : Entity
    {
        public Artifact() 
        {
            Name = nameof(Artifact);
            Emissions = new EmissionSignature();
            Resistances = new Resistances();
            Health = new Health();
            States = new States();
        }

        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public double BaseWeight { get; protected set; }
        public virtual double Weight => BaseWeight;

        [JsonProperty] public EmissionSignature Emissions { get; protected set; }
        [JsonProperty] public Resistances Resistances { get; protected set; }
        [JsonProperty] public Health Health { get; protected set; }
        [JsonProperty] public States States { get; protected set; }

        [Moddable] public bool Destroyed { get => Resolve(nameof(Destroyed)) > 0; }

        [Ability]
        public TurnAction Destroy()
        {
            return new TurnAction(0, 0, 0);
        }
    }
}
