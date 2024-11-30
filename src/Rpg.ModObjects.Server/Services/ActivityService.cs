using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.ModObjects.Server.Services
{
    public class ActivityService
    {
        private readonly GraphService _graphService;

        public ActivityService(GraphService graphService)
        {
            _graphService = graphService;
        }

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

            var activity = initiator.InitiateAction(owner, createActivity.ActionTemplate);
            if (!activity.Actions.Any())
                throw new InvalidOperationException($"Could not find action {createActivity.ActionTemplate} for owner {createActivity.OwnerId}");

            return activity;
        }

        public Activity AddAction(RpgGraph graph, ActionRef actionRef)
        {
            var activity = graph.GetObject<Activity>(actionRef.ActivityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {actionRef.ActivityId} in hydrated graph");

            var initiator = graph.GetObject<RpgEntity>(activity.OwnerId);
            if (initiator == null)
                throw new InvalidOperationException($"Could not find initiator with Id {activity.OwnerId} in hydrated graph");

            initiator.InitiateAction(actionRef);

            return activity;
        }
        public RpgArg[] GetActionArgs(RpgGraph graph, ActionStepArgs runStep)
        {
            var activity = graph.GetObject<Activity>(runStep.ActivityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {runStep.ActivityId} in hydrated graph");

            return runStep.ActionStep switch
            {
                ActionStep.Cost => activity.CostArgs(),
                ActionStep.Perform => activity.PerformArgs(),
                ActionStep.Outcome => activity.OutcomeArgs()
            };
        }

        public Activities.Action Cost(RpgGraph graph, ActionStepRun runStep)
        {
            var activity = graph.GetObject<Activity>(runStep.ActivityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {runStep.ActivityId} in hydrated graph");

            activity.Cost(runStep.Args);
            return activity.CurrentAction()!;
        }

        public Activities.Action Perform(RpgGraph graph, ActionStepRun runStep)
        {
            var activity = graph.GetObject<Activity>(runStep.ActivityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {runStep.ActivityId} in hydrated graph");

            activity.Perform(runStep.Args);
            return activity.CurrentAction()!;
        }

        public Activities.Action Outcome(RpgGraph graph, ActionStepRun runStep)
        {
            var activity = graph.GetObject<Activity>(runStep.ActivityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {runStep.ActivityId} in hydrated graph");

            activity.Outcome(runStep.Args);
            return activity.CurrentAction()!;
        }

        public Activities.Action Complete(RpgGraph graph, string activityId)
        {
            var activity = graph.GetObject<Activity>(activityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {activityId} in hydrated graph");

            activity.Complete();
            return activity.CurrentAction()!;
        }

        public Activities.Action AutoComplete(RpgGraph graph, string activityId)
        {
            var activity = graph.GetObject<Activity>(activityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {activityId} in hydrated graph");

            activity.AutoComplete();
            return activity.CurrentAction()!;
        }

        public Activities.Action Reset(RpgGraph graph, string activityId)
        {
            var activity = graph.GetObject<Activity>(activityId);
            if (activity == null)
                throw new InvalidOperationException($"Could not find activity with Id {activityId} in hydrated graph");

            activity.Reset();
            return activity.CurrentAction()!;
        }
    }
}
