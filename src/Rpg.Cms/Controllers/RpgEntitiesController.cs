using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Server;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Ops;
using Rpg.ModObjects.Time;

namespace Rpg.Cms.Controllers
{
    [Route("api/rpg")]
    [ApiController]
    public class RpgEntitiesController : Controller
    {
        private readonly RpgSessionlessServer _sessionlessServer;

        public RpgEntitiesController(RpgSessionlessServer sessionlessServer) 
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
        [HttpPost("{system}/entities/props/override")]
        [ProducesResponseType(typeof(RpgResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult OverrideBaseValue(string system, RpgRequest<OverrideBaseValue> request)
        {
            var response = _sessionlessServer.OverrideBaseValue(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/time")]
        [ProducesResponseType(typeof(RpgResponse<PointInTime>), StatusCodes.Status200OK)]
        public IActionResult SetTime(string system, RpgRequest<PointInTime> request)
        {
            var response = _sessionlessServer.SetTime(system, request);

            var json = RpgJson.Serialize(response);
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
        [HttpPost("{system}/modset/describe")]
        [ProducesResponseType(typeof(RpgResponse<ModSetDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeModSet(string system, RpgRequest<DescribeModSet> request)
        {
            var response = _sessionlessServer.Describe(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }

        [HttpPost("{system}/describe")]
        [ProducesResponseType(typeof(RpgResponse<PropDescription>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Describe(string system, RpgRequest<DescribeProp> request)
        {
            var response = _sessionlessServer.Describe(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
