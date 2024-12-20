﻿using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Description;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Server.Ops;
using Rpg.ModObjects.Server.Services;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Server
{
    public class RpgSessionlessServer
    {
        private readonly GraphService _graphService;
        private readonly ActivityService _activityService;
        private readonly EntityService _entityService;
        private readonly IContentFactory _contentFactory;

        public RpgSessionlessServer(GraphService graphService, ActivityService activityService, EntityService entityService, IContentFactory contentFactory)
        {
            _graphService = graphService;
            _activityService = activityService;
            _entityService = entityService;
            _contentFactory = contentFactory;
        }

        public RpgContent[] ListEntities(string system)
            => _contentFactory.ListEntities(system);

        public RpgResponse<string> CreateGraphState(string system, string archetype, string id)
        {
            var graph = _graphService.CreateGraph(system, archetype, id);
            return new RpgResponse<string>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = id
            };
        }

        public RpgResponse<bool> OverrideBaseValue(string system, RpgRequest<OverrideBaseValue> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var res = _entityService.OverrideBaseValue(graph, request.Op);
            return new RpgResponse<bool>
            {
                GraphState = graph.GetGraphState(),
                Data = res
            };
        }

        public RpgResponse<PointInTime> SetTime(string system, RpgRequest<PointInTime> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            graph.Time.Transition(request.Op);

            return new RpgResponse<PointInTime>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = graph.Time.Now
            };
        }

        public RpgResponse<string> SetState(string system, RpgRequest<SetState> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);

            var entity = graph.GetObject<RpgEntity>(request.Op.EntityId)!;
            if (request.Op.On)
                entity.SetStateOn(request.Op.State);
            else
                entity.SetStateOff(request.Op.State);

            graph.Time.TriggerEvent();

            return new RpgResponse<string>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = request.Op.EntityId
            };
        }

        public RpgResponse<ObjectPropInfo> Describe(string system, RpgRequest<DescribeProp> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var entity = graph.GetObject(request.Op.EntityId)!;
            if (entity == null)
                throw new InvalidOperationException($"Could not find entity with Id {request.Op.EntityId} in hydrated graph");

            var description = ObjectPropDescriber.Describe(graph, entity, request.Op.Prop);
            if (description == null)
                throw new InvalidOperationException($"Desription for Prop {request.Op.EntityId}.{request.Op.Prop} not found");

            return new RpgResponse<ObjectPropInfo>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = description!
            };
        }

        public RpgResponse<ModSetValues> Describe(string system, RpgRequest<DescribeModSet> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var entity = graph.GetObject(request.Op.EntityId)!;
            if (entity == null)
                throw new InvalidOperationException($"Could not find entity with Id {request.Op.EntityId} in hydrated graph");

            var description = ObjectPropDescriber.Values(graph, entity.GetModSet(request.Op.ModSetId)!);
            if (description == null)
                throw new InvalidOperationException($"Desription for ModSet {request.Op.EntityId}.{request.Op.ModSetId} not found");

            return new RpgResponse<ModSetValues>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = description!
            };
        }

        public RpgResponse<ModSetValues> Describe(string system, RpgRequest<DescribeState> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var entity = graph.GetObject(request.Op.EntityId)!;
            if (entity == null)
                throw new InvalidOperationException($"Could not find entity with Id {request.Op.EntityId} in hydrated graph");

            var description = ObjectPropDescriber.Values(graph, entity.GetState(request.Op.State)!);
            if (description == null)
                throw new InvalidOperationException($"Desription for State {request.Op.EntityId}.{request.Op.State} not found");

            return new RpgResponse<ModSetValues>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = description!
            };
        }

        public RpgResponse<Activity> InitiateAction(string system, RpgRequest<InitiateAction> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.InitiateAction(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<RpgArg[]> GetActionStepArgs(string system, RpgRequest<ActionStepArgs> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            return new RpgResponse<RpgArg[]>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = _activityService.GetActionArgs(graph, request.Op)
            };
        }

        public RpgResponse<Activities.Action> ActionExecuteCost(string system, RpgRequest<ActionStepRun> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.Cost(graph, request.Op);
            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<Activities.Action> ActionExecutePerform(string system, RpgRequest<ActionStepRun> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.Perform(graph, request.Op);

            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<Activities.Action> ActionExecuteOutcome(string system, RpgRequest<ActionStepRun> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.Outcome(graph, request.Op);

            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<Activities.Action> ActionComplete(string system, RpgRequest<ActivityComplete> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.Complete(graph, request.Op.ActivityId);

            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<Activities.Action> ActionAutoComplete(string system, RpgRequest<ActivityComplete> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.AutoComplete(graph, request.Op.ActivityId);

            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<Activities.Action> ActionReset(string system, RpgRequest<ActivityComplete> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var action = _activityService.Reset(graph, request.Op.ActivityId);

            return new RpgResponse<Activities.Action>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = action
            };
        }

        public RpgResponse<bool> ApplyModSet(string system, RpgRequest<ModSet> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);

            var owner = graph.GetObject<RpgEntity>(request.Op.OwnerId)!;
            var res = owner.AddModSet(request.Op);
            if (res)
                graph.Time.TriggerEvent();

            return new RpgResponse<bool>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = res
            };
        }
    }
}
