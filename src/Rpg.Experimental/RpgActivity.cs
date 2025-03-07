using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public class RpgActivity : RpgObject
    {
        [JsonProperty] public List<RpgObject> ActivityActions { get; protected set; } = new();
        [JsonIgnore] public RpgActivityAction? CurrentActivityAction { get => ActivityActions.FirstOrDefault() as RpgActivityAction; }
        public RpgArg[] Args { get; protected set; } = [];

        public RpgActivity(RpgObject owner, TimePoint start, TimePoint end)
            : base($"{owner.Id}_Activity", owner.Id, start, end)
        { }

        public RpgActivityAction CreateActivityAction(RpgGraph graph, RpgAction action)
        {
            var activityAction = new RpgActivityAction(this, action, ActivityActions.Count() + 1);
            action.OnCreatingActivityAction(graph, activityAction);

            graph.Add(activityAction);
            graph.AddTo(Id, nameof(ActivityActions), activityAction.Id, Start, End);

            graph.OnTemporalEvent([activityAction, this]);
            RpgArg.SetValues(graph, Args, activityAction);

            graph.ChangeTracker.SyncProperties(graph, activityAction.Id);
            graph.ChangeTracker.SyncProperties(graph, Id);

            return activityAction;
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);

            foreach (var activityAction in ActivityActions)
                RpgArg.SetValues(graph, Args, activityAction);
        }

        public override void OnSyncProperty(RpgGraph graph, string prop)
        {
            if (prop == nameof(ActivityActions))
                foreach (var activityAction in ActivityActions)
                    RpgArg.SetValues(graph, Args, activityAction);
        }
    }
}
