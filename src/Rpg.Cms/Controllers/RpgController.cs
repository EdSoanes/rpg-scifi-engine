using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Controllers.Services;
using Rpg.Cms.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Umbraco.Cms.Api.Common.Attributes;

namespace Rpg.Cms.Controllers
{
    [Route("api/v{version:apiVersion}/rpg")]
    [ApiController]
    [ApiVersion("1.0")]
    [MapToApi("rpg")]
    [ApiExplorerSettings(GroupName = "Entities")]
    public class RpgController : Controller
    {
        private readonly IGraphFactory _graphFactory;
        private readonly IContentFactory _contentFactory;

        public RpgController(IGraphFactory graphFactory, IContentFactory contentFactory) 
        {
            _graphFactory = graphFactory;
            _contentFactory = contentFactory;
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/entities")]
        [ProducesResponseType(typeof(RpgContent[]), StatusCodes.Status200OK)]
        public IActionResult ListEntities(string system)
        {
            var items = _contentFactory.ListEntities(system);
            return Ok(items);
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/{archetype}/{id}")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult CreateGraphState(string system, string archetype, string id)
        {
            var graphState = _graphFactory.CreateGraphState(system, archetype, id);
            return Ok(graphState);
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/actioninstance/create")]
        [ProducesResponseType(typeof(ActionInstance), StatusCodes.Status200OK)]
        public IActionResult GetActionInstance(string system, RpgOperation<CreateActionInstance> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var owner = graph.GetObject<RpgEntity>(op.Operation.OwnerId)!;
            var initiator = graph.GetObject<RpgEntity>(op.Operation.InitiatorId)!;

            var instance = owner.CreateActionInstance(initiator, op.Operation.ActionName, op.Operation.ActionNo);

            return Ok(instance);
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/modset")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult AddModSet(string system, RpgOperation<ModSet> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var owner = graph.GetObject<RpgEntity>(op.Operation.OwnerId)!;
            owner.AddModSet(op.Operation);
            graph.Time.TriggerEvent();

            var graphState = graph.GetGraphState();
            return Ok(graphState);
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/actioninstance/cost")]
        [ProducesResponseType(typeof(ModSet), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Cost(string system, RpgOperation<Act> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var owner = graph.GetObject<RpgEntity>(op.Operation.OwnerId)!;
            var initiator = graph.GetObject<RpgEntity>(op.Operation.InitiatorId)!;

            var instance = owner.CreateActionInstance(initiator, op.Operation.ActionName, op.Operation.ActionNo)!;
            instance.SetArgValues(op.Operation.ArgValues);

            return instance.CanAct()
                ? Ok(instance.Cost())
                : Forbid();
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/actioninstance/act")]
        [ProducesResponseType(typeof(ActionModSet), StatusCodes.Status200OK)]
        public IActionResult Act(string system, RpgOperation<Act> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var owner = graph.GetObject<RpgEntity>(op.Operation.OwnerId)!;
            var initiator = graph.GetObject<RpgEntity>(op.Operation.InitiatorId)!;

            var instance = owner.CreateActionInstance(initiator, op.Operation.ActionName, op.Operation.ActionNo)!;
            instance.SetArgValues(op.Operation.ArgValues);
            return Ok(instance.Act());
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/actioninstance/outcome")]
        [ProducesResponseType(typeof(ModSet[]), StatusCodes.Status200OK)]
        public IActionResult Outcome(string system, RpgOperation<Act> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var owner = graph.GetObject<RpgEntity>(op.Operation.OwnerId)!;
            var initiator = graph.GetObject<RpgEntity>(op.Operation.InitiatorId)!;

            var instance = owner.CreateActionInstance(initiator, op.Operation.ActionName, op.Operation.ActionNo)!;
            instance.SetArgValues(op.Operation.ArgValues);
            return Ok(instance.Outcome());
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/state")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult StateState(string system, RpgOperation<SetState> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);

            var entity = graph.GetObject<RpgEntity>(op.Operation.EntityId)!;
            var stateChanged = op.Operation.On
                ? entity.SetStateOn(op.Operation.State)
                : entity.SetStateOff(op.Operation.State);

            graph.Time.TriggerEvent();

            var graphState = graph.GetGraphState();
            return Ok(graphState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// POST {system}/describe
        /// </remarks>
        /// <param name="system"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        [HttpPost("{system}/describe")]
        [ProducesResponseType(typeof(PropDesc), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Describe(string system, RpgOperation<Describe> op)
        {
            var graph = _graphFactory.HydrateGraph(system, op.GraphState);
            var entity = graph.GetObject<RpgEntity>(op.Operation.EntityId)!;

            var description = entity.Describe(op.Operation.Prop);
            if (description == null)
                return BadRequest($"Desription for {op.Operation.EntityId}.{op.Operation.Prop} not found");

            return Ok(description);
        }
    }
}
