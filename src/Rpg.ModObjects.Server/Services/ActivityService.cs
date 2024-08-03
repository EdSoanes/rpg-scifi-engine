using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IGraphService _graphService;

        public ActivityService(IGraphService graphService)
        {
            _graphService = graphService;
        }

        public RpgActivity Create(RpgGraph graph, ActivityCreate createActivity)
        {
            var initiator = graph.GetObject<RpgEntity>(createActivity.InitiatorId);

            if (initiator == null)
                throw new InvalidOperationException($"Could not find initiator with Id {createActivity.InitiatorId} in hydrated graph");

            var owner = createActivity.OwnerId == createActivity.InitiatorId
                ? initiator
                : graph.GetObject<RpgEntity>(createActivity.OwnerId);

            if (owner == null)
                throw new InvalidOperationException($"Could not find owner with Id {createActivity.OwnerId} in hydrated graph");

            var activity = new RpgActivity(initiator, 0);
            graph.AddEntity(activity);

            activity.CreateActionInstance(owner, createActivity.Action);
            if (activity.ActionInstance == null)
                throw new InvalidOperationException($"Could not find action {createActivity.Action} for owner {createActivity.OwnerId}");

            activity.Cost();

            return activity;
        }

        public RpgActivity Act(RpgGraph graph, ActivityAct activityAct)
            => RunActivityStep(graph, activityAct.ActivityId, (activity) => activity.Act());

        public RpgActivity Outcome(RpgGraph graph, ActivityOutcome activityOutcome)
            => RunActivityStep(graph, activityOutcome.ActivityId, (activity) => activity.Outcome());

        public RpgActivity AutoComplete(RpgGraph graph, ActivityAutoComplete activityAutoComplete)
            => RunActivityStep(graph, activityAutoComplete.ActivityId, (activity) => activity.AutoComplete());

        public RpgActivity Complete(RpgGraph graph, ActivityComplete activityComplete)
            => RunActivityStep(graph, activityComplete.ActivityId, (activity) => activity.Complete());

        private RpgActivity RunActivityStep(RpgGraph graph, string activityId, Action<RpgActivity> runStep)
        {
            var activity = graph.GetObject<RpgActivity>(activityId);

            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {activityId} in hydrated graph");

            if (activity.ActionInstance == null)
                throw new InvalidOperationException($"Could not find action instance for activity {activityId}");

            runStep(activity);

            return activity;
        }
    }
}
