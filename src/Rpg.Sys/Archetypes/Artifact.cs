using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public abstract class Artifact : RpgEntity
    {
        [JsonProperty] 
        [ComponentUI(Group = "Health")]
        public Health Health { get; private set; }

        [JsonProperty] 
        [ComponentUI(Group = "Presence")]
        public Presence Presence { get; private set; }

        [JsonProperty] 
        [ComponentUI(Tab = "Combat", Group = "Attack")]
        public Damage Damage { get; private set; }

        [JsonProperty] 
        [ComponentUI(Tab = "Combat", Group = "Defense")]
        public Defenses Defenses { get; private set; }

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
