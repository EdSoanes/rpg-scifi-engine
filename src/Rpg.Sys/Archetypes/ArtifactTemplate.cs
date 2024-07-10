using Rpg.ModObjects;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public class ArtifactTemplate : IRpgEntityTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PresenceTemplate Presence {  get; set; } = new PresenceTemplate();
        public HealthTemplate Health { get; set; } = new HealthTemplate();
        public DamageTemplate Damage { get; set; } = new DamageTemplate();
        public DefensesTemplate Defenses { get; set; } = new DefensesTemplate();
    }
}
