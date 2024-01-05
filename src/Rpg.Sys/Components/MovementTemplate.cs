using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class MovementTemplate
    {
        public int MaxSpeed { get; set; }
        public int Acceleration { get; set; }
        public int Deceleration { get; set; }
    }
}
