using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Abilities
{
    public class Jump : Ability
    {
        public Jump()
        {
            Name = "Jump";
            Description = "How far can you jump?";
            Exertion = 5;
            ActionPointCost = 2;
        }
    }
}
