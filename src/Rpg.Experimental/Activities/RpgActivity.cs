using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Activities
{
    public class RpgActivity : RpgObject
    {
        [JsonProperty] public List<RpgObject> ActivityActions { get; protected set; } = new(); 
        [JsonProperty] public RpgArg[] ActionArgs { get; protected set; } = [];

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

            foreach (var activityAction in ActivityActions.Cast<RpgActivityAction>())
                ActionArgs = RpgArg.SyncArgs(ActionArgs,
                    activityAction.CostMethod.Args,
                    activityAction.PerformMethod.Args,
                    activityAction.OutcomeMethod.Args
                    );
        }
    }
}
