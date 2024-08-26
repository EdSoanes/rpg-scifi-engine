using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Server.Ops;
using Rpg.ModObjects.Server.Services;

namespace Rpg.ModObjects.Server
{
    public class RpgSessionlessServer : IRpgSessionlessServer
    {
        private readonly IGraphService _graphService;
        private readonly IActivityService _activityService;
        private readonly IContentFactory _contentFactory;

        public RpgSessionlessServer(IGraphService graphService, IActivityService activityService, IContentFactory contentFactory)
        {
            _graphService = graphService;
            _activityService = activityService;
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

        public RpgResponse<PropDesc> Describe(string system, RpgRequest<Describe> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var entity = graph.GetObject<RpgEntity>(request.Op.EntityId)!;
            if (entity == null)
                throw new InvalidOperationException($"Could not find entity with Id {request.Op.EntityId} in hydrated graph");

            var description = entity.Describe(request.Op.Prop);
            if (description == null)
                throw new InvalidOperationException($"Desription for {request.Op.EntityId}.{request.Op.Prop} not found");

            return new RpgResponse<PropDesc>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = description!
            };
        }

        public ActionGroup[] ActivityActionGroups(string system)
            => _activityService.GetActionGroups(system);

        public RpgResponse<Activity> ActivityCreate(string system, RpgRequest<ActivityCreate> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.Create(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<Activity> ActivityCreate(string system, RpgRequest<ActivityCreateByGroup> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.Create(system, graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<Activity> ActivityAct(string system, RpgRequest<ActivityAct> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.Act(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<Activity> ActivityOutcome(string system, RpgRequest<ActivityOutcome> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.Outcome(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<Activity> ActivityAutoComplete(string system, RpgRequest<ActivityAutoComplete> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.AutoComplete(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
            };
        }

        public RpgResponse<Activity> ActivityComplete(string system, RpgRequest<ActivityComplete> request)
        {
            var graph = _graphService.HydrateGraph(system, request.GraphState);
            var activity = _activityService.Complete(graph, request.Op);

            return new RpgResponse<Activity>
            {
                GraphState = _graphService.DehydrateGraph(graph),
                Data = activity
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
