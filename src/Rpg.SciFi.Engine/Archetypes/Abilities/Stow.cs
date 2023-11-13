using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class Stow : Ability
    {
        public Stow()
        {
            Name = "Stow";
            Description = "Put an item in the container";
            ActionPointCost = 2;
            Exertion = 1;
        }
    }
}
