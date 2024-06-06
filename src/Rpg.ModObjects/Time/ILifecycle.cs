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
        ModExpiry Expiry { get; }

        void SetExpired(TimePoint currentTime);
        ModExpiry StartLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null);
        ModExpiry UpdateLifecycle(RpgGraph graph, TimePoint currentTime, Mod? mod = null);
    }
}
