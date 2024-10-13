using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta;
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

        public ActivityTemplate[] GetActivityTemplates(string systemIdentifier)
            => MetaSystems.Get(systemIdentifier)!.ActivityTemplates;

        public Activity Create(RpgGraph graph, ActivityCreate createActivity)
        {
            var initiator = graph.GetObject<RpgEntity>(createActivity.InitiatorId);

            if (initiator == null)
                throw new InvalidOperationException($"Could not find initiator with Id {createActivity.InitiatorId} in hydrated graph");

            var owner = createActivity.OwnerId == createActivity.InitiatorId
                ? initiator
                : graph.GetObject<RpgEntity>(createActivity.OwnerId);

            if (owner == null)
                throw new InvalidOperationException($"Could not find owner with Id {createActivity.OwnerId} in hydrated graph");

            var activity = graph.CreateActivity(initiator, owner, createActivity.Action);
            if (activity.ActionInstance == null)
                throw new InvalidOperationException($"Could not find action {createActivity.Action} for owner {createActivity.OwnerId}");

            activity.Cost();

            return activity;
        }

        public Activity Create(string systemIdentifier, RpgGraph graph, ActivityCreateByTemplate createActivity)
        {
            var initiator = graph.GetObject<RpgEntity>(createActivity.InitiatorId);

            if (initiator == null)
                throw new InvalidOperationException($"Could not find initiator with Id {createActivity.InitiatorId} in hydrated graph");

            var owner = createActivity.OwnerId == createActivity.InitiatorId
                ? initiator
                : graph.GetObject<RpgEntity>(createActivity.OwnerId);

            if (owner == null)
                throw new InvalidOperationException($"Could not find owner with Id {createActivity.OwnerId} in hydrated graph");

            var actionGroup = GetActivityTemplates(systemIdentifier).FirstOrDefault(x => x.Name == createActivity.ActivityTemplateName);
            if (actionGroup == null)
                throw new InvalidOperationException($"Could not find activity template {createActivity.ActivityTemplateName}");

            var activity = graph.CreateActivity(initiator, actionGroup);
            if (activity.ActionInstance == null)
                throw new InvalidOperationException($"Could not create instance for activity template {createActivity.ActivityTemplateName} for owner {createActivity.OwnerId}");

            activity.Cost();

            return activity;
        }

        public Activity Act(RpgGraph graph, ActivityAct activityAct)
            => RunActivityStep(graph, activityAct.ActivityId, (activity) => activity.Act());

        public Activity Outcome(RpgGraph graph, ActivityOutcome activityOutcome)
            => RunActivityStep(graph, activityOutcome.ActivityId, (activity) => activity.Outcome());

        public Activity AutoComplete(RpgGraph graph, ActivityAutoComplete activityAutoComplete)
            => RunActivityStep(graph, activityAutoComplete.ActivityId, (activity) => activity.AutoComplete());

        public Activity Complete(RpgGraph graph, ActivityComplete activityComplete)
            => RunActivityStep(graph, activityComplete.ActivityId, (activity) => activity.Complete());

        private Activity RunActivityStep(RpgGraph graph, string activityId, System.Action<Activity> runStep)
        {
            var activity = graph.GetObject<Activity>(activityId);

            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {activityId} in hydrated graph");

            if (activity.ActionInstance == null)
                throw new InvalidOperationException($"Could not find action instance for activity {activityId}");

            runStep(activity);

            return activity;
        }
    }
}
