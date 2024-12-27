using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Time
{
    public interface ILifecycle
    {
        void OnCreating(RpgGraph graph, RpgObject obj);
        void OnTimeEvent(TimePoint now);
    }
}
