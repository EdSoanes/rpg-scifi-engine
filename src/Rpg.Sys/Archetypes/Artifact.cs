using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public abstract class Artifact : RpgEntity
    {
        [JsonProperty] 
        [Component(Group = "Health")]
        public Health Health { get; private set; }

        [JsonProperty] 
        [Component(Group = "Presence")]
        public Presence Presence { get; private set; }

        [JsonProperty] 
        [Component(Tab = "Combat", Group = "Attack")]
        public Damage Damage { get; private set; }

        [JsonProperty] 
        [Component(Tab = "Combat", Group = "Defense")]
        public Defenses Defenses { get; private set; }

        [JsonConstructor] protected Artifact() { }

        public Artifact(ArtifactTemplate template)
        {
            if (!string.IsNullOrEmpty(template.Name))
                Name = template.Name;

            Presence = new Presence(Id, nameof(Presence), template.Presence);
            Defenses = new Defenses(Id, nameof(Defenses), template.Defenses);
            Damage = new Damage(Id, nameof(Damage), template.Damage);
            Health = new Health(Id, nameof(Health));
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, this);
            Damage.OnBeforeTime(graph, this);
            Defenses.OnBeforeTime(graph, this);
            Presence.OnBeforeTime(graph, this);
            Health.OnBeforeTime(graph, this);
        }
    }
}
