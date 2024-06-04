using Rpg.ModObjects.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public interface ITimeLifecycle
    {
        ModExpiry Expiry { get; }

        void SetExpired(Time currentTime);
        ModExpiry StartLifecycle(RpgGraph graph, Time currentTime, Mod? mod = null);
        ModExpiry UpdateLifecycle(RpgGraph graph, Time currentTime, Mod? mod = null);
    }
}
