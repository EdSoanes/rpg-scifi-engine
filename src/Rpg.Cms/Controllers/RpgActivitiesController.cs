using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Server;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Ops;
using Umbraco.Cms.Api.Common.Attributes;

namespace Rpg.Cms.Controllers
{
    [Route("api/rpg")]
    [ApiController]
    public class RpgActivitiesController : Controller
    {
        private readonly RpgSessionlessServer _sessionlessServer;

        public RpgActivitiesController(RpgSessionlessServer sessionlessServer) 
        {
            _sessionlessServer = sessionlessServer;
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/initiate")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult InitiateAction(string system, [FromBody] RpgRequest<InitiateAction> request)
        {
            var response = _sessionlessServer.InitiateAction(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/args")]
        [ProducesResponseType(typeof(RpgResponse<RpgArg[]>), StatusCodes.Status200OK)]
        public IActionResult ActionStepArgs(string system, RpgRequest<ActionStepArgs> request)
        {
            var response = _sessionlessServer.GetActionStepArgs(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }
        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/cost")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionCost(string system, RpgRequest<ActionStepRun> request)
        {
            var response = _sessionlessServer.ActionExecuteCost(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/perform")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionPerform(string system, RpgRequest<ActionStepRun> request)
        {
            var response = _sessionlessServer.ActionExecutePerform(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/outcome")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionOutcome(string system, RpgRequest<ActionStepRun> request)
        {
            var response = _sessionlessServer.ActionExecuteOutcome(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/complete")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionComplete(string system, RpgRequest<ActivityComplete> request)
        {
            var response = _sessionlessServer.ActionComplete(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/autocomplete")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionAutoComplete(string system, RpgRequest<ActivityComplete> request)
        {
            var response = _sessionlessServer.ActionAutoComplete(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }


        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/action/reset")]
        [ProducesResponseType(typeof(RpgResponse<ModObjects.Activities.Action>), StatusCodes.Status200OK)]
        public IActionResult ActionReset(string system, RpgRequest<ActivityComplete> request)
        {
            var response = _sessionlessServer.ActionReset(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
