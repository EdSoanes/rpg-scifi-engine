using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public interface ITimeEvent
    {
        void OnUpdating(RpgGraph graph, TimePoint time);
    }
}
