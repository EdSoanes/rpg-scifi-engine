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
        void OnTimeEvent(RpgGraph graph);
        void Expire(RpgGraph graph, TimePoint expiryTime);
        void Expire(RpgGraph graph);
        void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId);
    }
}
