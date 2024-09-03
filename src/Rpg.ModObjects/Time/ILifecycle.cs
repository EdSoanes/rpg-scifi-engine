namespace Rpg.ModObjects.Time
{
    public interface ILifecycle
    {
        LifecycleExpiry Expiry { get; }
        void OnCreating(RpgGraph graph, RpgObject? entity = null);
        void OnRestoring(RpgGraph graph, RpgObject? entity = null);
        LifecycleExpiry OnStartLifecycle();
        void OnTimeBegins();
        LifecycleExpiry OnUpdateLifecycle();
    }
}