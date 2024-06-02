using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Modifiers
{
    public class PermanentLifecycle : ITimeLifecycle
    { 
        public ModExpiry Expiry { get => ModExpiry.Active;  }
        public void SetExpired()
        {
        }

        public ModExpiry StartLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
                => ModExpiry.Active;

        public ModExpiry SyncWith(ITimeLifecycle lifecycle)
            => ModExpiry.Active;

        public ModExpiry UpdateLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
                => ModExpiry.Active;
    }
}
