using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class Take : Ability
    {
        public Take()
        {
            Name = "Take";
            Description = "Take an item from the container";
            ActionPointCost = 2;
            Exertion = 1;
        }
    }
}
