using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Server;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Ops;
using Umbraco.Cms.Api.Common.Attributes;

namespace Rpg.Cms.Controllers
{
    [Route("api/v{version:apiVersion}/rpg")]
    [ApiController]
    [ApiVersion("1.0")]
    [MapToApi("rpg")]
    [ApiExplorerSettings(GroupName = "Activities")]
    public class RpgActivitiesController : Controller
    {
        private readonly RpgSessionlessServer _sessionlessServer;

        public RpgActivitiesController(RpgSessionlessServer sessionlessServer) 
        {
            _sessionlessServer = sessionlessServer;
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/create")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityCreate(string system, [FromBody] RpgRequest<ActivityCreate> request)
        {
            var response = _sessionlessServer.ActivityCreate(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/createbygroup")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityCreateByGroup(string system, RpgRequest<ActivityCreateByTemplate> request)
        {
            var response = _sessionlessServer.ActivityCreate(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/activity/templates")]
        [ProducesResponseType(typeof(ActivityTemplate[]), StatusCodes.Status200OK)]
        public IActionResult ActivityTemplates(string system)
        {
            var response = _sessionlessServer.ActivityTemplates(system);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/act")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityAct(string system, RpgRequest<ActivityAct> request)
        {
            var response = _sessionlessServer.ActivityAct(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/outcome")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityOutcome(string system, RpgRequest<ActivityOutcome> request)
        {
            var response = _sessionlessServer.ActivityOutcome(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
