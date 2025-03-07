using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public abstract class RpgGraphState
    {
        public List<RpgLifecycleObject> Objects { get; set; } = new();
        public List<RpgObjectData> ObjectData { get; set; } = new();
        public string ContextId { get; set; }
        public Temporal Time { get; set; }
    }
}
