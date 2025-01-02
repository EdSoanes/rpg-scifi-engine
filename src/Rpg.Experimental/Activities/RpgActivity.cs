using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Activities
{
    public class RpgActivity : RpgObject
    {
        [JsonProperty] public List<RpgObject> ActivityActions { get; protected set; } = new();

        public RpgActivity(RpgObject owner, TimePoint start, TimePoint end)
            : base($"{owner.Id}_Activity", owner.Id, start, end)
        {
        }


        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
        }
    }
}
