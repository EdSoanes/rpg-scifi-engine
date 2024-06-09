using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public abstract class Artifact : RpgEntity
    {
        [JsonProperty] public Health Health { get; private set; }

        [JsonProperty] public Presence Presence { get; private set; }

        [JsonProperty] public Defenses Defenses { get; private set; }

        [JsonProperty] public Damage Damage { get; private set; }

        [JsonConstructor] protected Artifact() { }

        public Artifact(ArtifactTemplate template)
        {
            if (!string.IsNullOrEmpty(template.Name))
                Name = template.Name;

            Presence = new Presence(Id, nameof(Presence), template.Presence);
            Defenses = new Defenses(Id, nameof(Defenses), template.Defenses);
            Damage = new Damage(Id, nameof(Damage), template.Damage);
            Health = new Health(Id, nameof(Health), template.Health);
        }
    }
}
