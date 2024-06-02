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
        void SetExpired();
        ModExpiry SyncWith(ITimeLifecycle lifecycle);
        ModExpiry StartLifecycle<T>(RpgGraph graph, Time time, T obj)
            where T : class;
        ModExpiry UpdateLifecycle<T>(RpgGraph graph, Time time, T obj)
            where T : class;
    }
}
