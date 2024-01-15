using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rpg.Sys.Archetypes
{
    public class OnState : State<Equipment>
    {
        public OnState(Guid id)
            : base(id, "On")
        {
        }

        protected override Condition OnActive(Actor actor, Equipment artifact)
        {
            var modSet = base.OnActive(actor, artifact)
                .Add(StateModifier.Create(Name, artifact, 1, x => x.Presence.Sound.Current));

            return modSet;
        }
    }
}
