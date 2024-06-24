using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class ActorTemplate : ArtifactTemplate
    {
        public string? Class { get; set; }

        [ComponentUI(Tab = "Stats")]
        public StatPointsTemplate Stats { get; set; } = new StatPointsTemplate();

        [ComponentUI]
        public MovementTemplate Movement { get; set; } = new MovementTemplate();

        [ComponentUI(Tab = "Actions")]
        public ActionPointsTemplate Actions { get; set; } = new ActionPointsTemplate();
    }
}
