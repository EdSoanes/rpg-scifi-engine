using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class PresenceValue : MaxCurrentValue
    {
        public int Radius { get; protected set; }

        public PresenceValue(int max, int current, int radius)
            : base(max, current) 
        {
            Radius = radius;
        }
    }
}
