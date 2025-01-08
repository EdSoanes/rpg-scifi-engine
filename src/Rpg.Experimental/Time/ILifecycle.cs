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
        void OnCreating(RpgGraph graph, RpgObject? obj);
        void OnRestoring(RpgGraph graph);
        void OnTimeEvent(RpgGraph graph);
        void OnSyncProperty(RpgGraph graph, string prop);
        
        void Expire(TimePoint expiryTime);
        void Expire(RpgGraph graph, TimePoint expiryTime);
        void Expire(RpgGraph graph);
        void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId);
    }
}
