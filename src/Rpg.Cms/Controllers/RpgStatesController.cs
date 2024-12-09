using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.ModObjects.Description;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Server;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Ops;

namespace Rpg.Cms.Controllers
{
    [Route("api/rpg")]
    [ApiController]
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
        [ProducesResponseType(typeof(RpgResponse<ModSetValues>), StatusCodes.Status200OK)]
        public IActionResult StateValues(string system, RpgRequest<DescribeState> request)
        {
            var response = _sessionlessServer.Describe(system, request);

            var json = RpgJson.Serialize(response);
            return new ContentResult() { Content = json, ContentType = "application/json" };
        }
    }
}
