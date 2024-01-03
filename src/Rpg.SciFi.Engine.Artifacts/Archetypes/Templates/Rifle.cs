using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes.Templates
{
    public class Rifle : GunTemplate
    {
        public Rifle() 
        {
            Range = 10;
            Attack = 10;
            Impact = "d6";
        }
    }
}
