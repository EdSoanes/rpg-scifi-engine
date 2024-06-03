using Rpg.ModObjects.Time;

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

        public ModExpiry UpdateLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
                => ModExpiry.Active;
    }

    public class SyncedLifecycle : ITimeLifecycle
    {
        public ModExpiry Expiry { get; private set; }

        public void SetExpired()
        {
        }

        public ModExpiry StartLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
        {
            var mod = obj as Mod;
            var modSet = graph.GetModSet(mod?.SyncedToId);

            Expiry = modSet?.Lifecycle.Expiry ?? ModExpiry.Remove;
            return Expiry;
        }

        public ModExpiry UpdateLifecycle<T>(RpgGraph graph, Time.Time time, T obj)
            where T : class
        {
            var mod = obj as Mod;
            var modSet = graph.GetModSet(mod?.SyncedToId);

            Expiry = modSet?.Lifecycle.Expiry ?? ModExpiry.Remove;
            return Expiry;
        }
    }
}
