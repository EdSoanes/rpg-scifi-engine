using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.Sys.Components;
using System.Text.Json.Serialization;

namespace Rpg.Sys.Archetypes
{
    public abstract class Artifact : RpgEntity
    {
        [JsonInclude] 
        [Component(Group = "Health")]
        public Health Health { get; private set; }

        [JsonInclude] 
        [Component(Group = "Presence")]
        public Presence Presence { get; private set; }

        [JsonInclude] 
        [Component(Tab = "Combat", Group = "Attack")]
        public Damage Damage { get; private set; }

        [JsonInclude] 
        [Component(Tab = "Combat", Group = "Defense")]
        public Defenses Defenses { get; private set; }

        [JsonConstructor] protected Artifact() { }

        public Artifact(ArtifactTemplate template)
        {
            if (!string.IsNullOrEmpty(template.Name))
                Name = template.Name;

            Presence = new Presence(nameof(Presence), template.Presence);
            Defenses = new Defenses(nameof(Defenses), template.Defenses);
            Damage = new Damage(nameof(Damage), template.Damage);
            Health = new Health(nameof(Health));
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, this);
            Damage.OnCreating(graph, this);
            Defenses.OnCreating(graph, this);
            Presence.OnCreating(graph, this);
            Health.OnCreating(graph, this);
        }
    }
}
