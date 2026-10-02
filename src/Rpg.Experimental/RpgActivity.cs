using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public class RpgActivity : RpgObject
    {
        [JsonProperty] public List<RpgObject> ActivityActions { get; protected set; } = new();
        /// <summary>
        /// The first activity action that has not been completed, otherwise the last one
        /// </summary>
        [JsonIgnore] public RpgActivityAction? CurrentActivityAction
        {
            get
            {
                var activityActions = ActivityActions.OfType<RpgActivityAction>().OrderBy(x => x.ActivityActionNo);
                return activityActions.FirstOrDefault(x => !x.IsComplete) ?? activityActions.LastOrDefault();
            }
        }

        /// <summary>
        /// The args of every action in the activity, merged by name. Values are propagated up from the
        /// activity actions on each refresh and flow down into new activity actions when they are created.
        /// </summary>
        [JsonProperty] public RpgArg[] Args { get; protected set; } = [];

        [JsonConstructor] protected RpgActivity() { }

        public RpgActivity(RpgObject owner, TimePoint start, TimePoint end)
            : base($"{owner.Id}_Activity", owner.Id, start, end)
        { }

        public RpgActivityAction CreateActivityAction(RpgGraph graph, RpgAction action)
        {
            var activityAction = new RpgActivityAction(this, action, ActivityActions.Count() + 1);

            graph.Add(activityAction);
            graph.AddTo(Id, nameof(ActivityActions), activityAction.Id, Start, End);

            //Values established by earlier actions in the activity (e.g. damage) flow down to the new action.
            //Where several earlier actions have a value for the same name the latest action wins.
            var carriedValues = new Dictionary<string, object?>();
            var earlierActions = ActivityActions
                .OfType<RpgActivityAction>()
                .Where(x => x.Id != activityAction.Id)
                .OrderBy(x => x.ActivityActionNo);

            foreach (var earlierAction in earlierActions)
                foreach (var value in graph.GetVirtualPropertyValues(earlierAction))
                    carriedValues[value.Item1] = value.Item2;

            graph.FillVirtualPropertyValues(activityAction, carriedValues.Select(x => (x.Key, x.Value)).ToArray());

            Args = RpgArg.CreateArgs(graph, Args,
                activityAction.CostMethod.Args,
                activityAction.PerformMethod.Args,
                activityAction.OutcomeMethod.Args);

            graph.OnTemporalEvent([activityAction, this]);

            graph.ChangeTracker.SyncProperties(graph, activityAction.Id);
            graph.ChangeTracker.SyncProperties(graph, Id);

            return activityAction;
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);
            SetArgValues(graph);
        }

        public override void OnSyncProperty(RpgGraph graph, string prop)
        {
            if (prop == nameof(ActivityActions))
                SetArgValues(graph);
        }

        private void SetArgValues(RpgGraph graph)
        {
            foreach (var activityAction in ActivityActions.OfType<RpgActivityAction>().OrderBy(x => x.ActivityActionNo))
                RpgArg.SetValues(graph, Args, activityAction);
        }
    }
}
