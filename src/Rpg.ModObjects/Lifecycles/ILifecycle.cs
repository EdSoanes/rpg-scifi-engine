using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Lifecycles
{
    public interface ILifecycle
    {
        LifecycleExpiry Expiry { get; }

        void SetExpired();

        void OnBeforeTime(RpgGraph graph, RpgObject? entity = null);
        void OnTimeBegins();
        LifecycleExpiry OnStartLifecycle();
        LifecycleExpiry OnUpdateLifecycle();
    }
}
