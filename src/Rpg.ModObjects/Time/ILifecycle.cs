using Rpg.ModObjects.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public interface ILifecycle
    {
        LifecycleExpiry Expiry { get; }

        void SetExpired(TimePoint currentTime);

        void OnBeginningOfTime(RpgGraph graph, RpgObject? entity = null);
        LifecycleExpiry OnStartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null);
        LifecycleExpiry OnUpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null);
    }
}
