using Asp.Versioning;
using Azure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
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
    [ApiExplorerSettings(GroupName = "Entities")]
    public class RpgController : Controller
    {
        private readonly IRpgSessionlessServer _sessionlessServer;

        public RpgController(IRpgSessionlessServer sessionlessServer) 
        {
            _sessionlessServer = sessionlessServer;
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/entities")]
        [ProducesResponseType(typeof(RpgContent[]), StatusCodes.Status200OK)]
        public IActionResult ListEntities(string system)
        {
            var items = _sessionlessServer.ListEntities(system);

            var json = RpgJson.Serialize(items);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/{archetype}/{id}")]
        [ProducesResponseType(typeof(RpgResponse<string>), StatusCodes.Status200OK)]
        public IActionResult CreateGraphState(string system, string archetype, string id)
        {
            var graphState = _sessionlessServer.CreateGraphState(system, archetype, id);

            var json = RpgJson.Serialize(graphState);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/modset")]
        [ProducesResponseType(typeof(RpgResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult AddModSet(string system, RpgRequest<ModSet> request)
        {
            var response = _sessionlessServer.ApplyModSet(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/create")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityCreate(string system, RpgRequest<ActivityCreate> request)
        {
            var response = _sessionlessServer.ActivityCreate(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/activity/createbygroup")]
        [ProducesResponseType(typeof(RpgResponse<Activity>), StatusCodes.Status200OK)]
        public IActionResult ActivityCreateByGroup(string system, RpgRequest<ActivityCreateByGroup> request)
        {
            var response = _sessionlessServer.ActivityCreate(system, request);

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

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/state")]
        [ProducesResponseType(typeof(RpgResponse<string>), StatusCodes.Status200OK)]
        public IActionResult StateState(string system, RpgRequest<SetState> request)
        {
            var response = _sessionlessServer.SetState(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };

        }

        [HttpPost("{system}/describe")]
        [ProducesResponseType(typeof(RpgResponse<PropDesc>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Describe(string system, RpgRequest<Describe> request)
        {
            var response = _sessionlessServer.Describe(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
