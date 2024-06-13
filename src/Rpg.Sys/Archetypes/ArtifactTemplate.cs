using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class ArtifactTemplate : IRpgEntityTemplate
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [ComponentUI]
        public PresenceTemplate Presence {  get; set; } = new PresenceTemplate();
        
        public HealthTemplate Health { get; set; } = new HealthTemplate();

        [ComponentUI(Tab = "Combat")]
        public DamageTemplate Damage { get; set; } = new DamageTemplate();

        [ComponentUI(Tab = "Combat")]
        public DefensesTemplate Defenses { get; set; } = new DefensesTemplate();
    }
}
