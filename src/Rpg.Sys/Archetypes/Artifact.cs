using Newtonsoft.Json;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public abstract class Artifact : ModdableObject
    {
        [JsonProperty] public Presence Presence { get; private set; }
        [JsonProperty] public Defenses Defenses { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }
        [JsonProperty] public Health Health { get; private set; }
        [JsonProperty] public States States { get; protected set; }

        [JsonConstructor] protected Artifact() { }

        public Artifact(ArtifactTemplate template)
        {
            if (!string.IsNullOrEmpty(template.Name))
                Name = template.Name;

            Presence = new Presence(template.Presence);
            Defenses = new Defenses(template.Defenses);
            Damage = new Damage(template.Damage);

            States = new States
            {
                template.States.ToArray()
            };

            Health = new Health(template.Health);
        }
    }
}
