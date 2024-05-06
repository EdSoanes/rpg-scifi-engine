using Rpg.Sys.Actions;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class Human : Actor
    {
        public Human(ActorTemplate template)
            : base(template) 
        { }

        public Container LeftHand { get; private set; } = new Container(nameof(LeftHand), new ActionCost(1, 0, 0), new ActionCost());
        public Container RightHand { get; private set; } = new Container(nameof(RightHand), new ActionCost(1, 0, 0), new ActionCost());
        public Container Equipment { get; private set; } = new Container(nameof(Equipment), new ActionCost(2, 1, 1), new ActionCost(2, 1, 1));
    }
}
