using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes.Actions
{
    public class Action
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Action() 
        {
            Name = GetType().Name;
            Description = GetType().Name;
        }
    }
}
