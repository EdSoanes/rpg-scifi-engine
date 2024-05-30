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

        public PresenceValue(Guid entityId, string name, int max, int current, int radius)
            : base(entityId, name, max, current)
                => Radius = radius;
    }
}
