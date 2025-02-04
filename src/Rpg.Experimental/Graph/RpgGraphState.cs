using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public class RpgGraphState
    {
        public List<RpgLifecycleObject> Objects { get; set; } = new();
        public List<RpgObjectData> ObjectData { get; set; } = new();
        public string ContextId { get; set; }
        public string InitiatorId { get; set; }
        public Temporal Time { get; set; }

        public RpgGraphState() { }
    }
}
