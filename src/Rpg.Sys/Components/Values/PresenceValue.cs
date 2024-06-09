using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class PresenceValue : MinMaxValue
    {
        public int Radius { get; protected set; }

        public PresenceValue(string entityId, string name, int max, int current, int radius)
            : base(entityId, name, max, current)
                => Radius = radius;
    }
}
