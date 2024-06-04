using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class ArtifactTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public PresenceTemplate Presence {  get; set; } = new PresenceTemplate();
        public DefensesTemplate Defenses { get; set; } = new DefensesTemplate();
        public DamageTemplate Damage { get; set; } = new DamageTemplate();
        public HealthTemplate Health { get; set; } = new HealthTemplate();
    }
}
