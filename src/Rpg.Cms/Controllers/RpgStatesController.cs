using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
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
    [ApiExplorerSettings(GroupName = "States")]
    public class RpgStatesController : Controller
    {
        private readonly RpgSessionlessServer _sessionlessServer;

        public RpgStatesController(RpgSessionlessServer sessionlessServer) 
        {
            _sessionlessServer = sessionlessServer;
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

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/state/describe")]
        [ProducesResponseType(typeof(RpgResponse<ModSetDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeState(string system, RpgRequest<DescribeState> request)
        {
            var response = _sessionlessServer.Describe(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
