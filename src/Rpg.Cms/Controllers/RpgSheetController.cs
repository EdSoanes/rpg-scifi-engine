using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.Experimental;
using Rpg.Experimental.Description;
using Rpg.Experimental.Server;
using Rpg.Experimental.System;

namespace Rpg.Cms.Controllers
{
    /// <summary>
    /// The game systems, their content libraries, and the operations a character sheet user interface
    /// performs on a character sheet.
    ///
    /// The server keeps no sheet. Each request carries the whole sheet as text and each response carries it
    /// back. The same operations can run on the device itself, with no server, through RpgSessionlessServer.
    /// </summary>
    [Route("api/rpg")]
    [ApiController]
    [RpgExceptionFilter]
    [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
    [Produces("application/json")]
    public class RpgSheetController : RpgControllerBase
    {
        private readonly RpgSessionlessServer _server;

        public RpgSheetController(RpgSessionlessServer server)
            => _server = server;

        #region Systems and content

        [HttpGet("systems")]
        [ProducesResponseType(typeof(RpgSystem[]), StatusCodes.Status200OK)]
        public IActionResult ListSystems()
            => RpgJson(_server.ListSystems());

        [HttpGet("{system}")]
        [ProducesResponseType(typeof(RpgSystem), StatusCodes.Status200OK)]
        public IActionResult GetSystem(string system)
            => RpgJson(_server.GetSystem(system));

        [HttpGet("{system}/entities")]
        [ProducesResponseType(typeof(RpgContent[]), StatusCodes.Status200OK)]
        public IActionResult ListEntities(string system)
            => RpgJson(_server.ListEntities(system));

        /// <summary>
        /// A new character sheet for a character or item in the content library
        /// </summary>
        [HttpGet("{system}/{archetype}/{id}")]
        [ProducesResponseType(typeof(RpgResponse<RpgSheetView>), StatusCodes.Status200OK)]
        public IActionResult CreateSheet(string system, string archetype, string id)
            => RpgJson(_server.CreateSheet(system, archetype, id));

        [HttpPost("{system}/sheet")]
        [ProducesResponseType(typeof(RpgResponse<RpgSheetView>), StatusCodes.Status200OK)]
        public IActionResult GetSheet(string system, [RpgBody] RpgRequest<object?> request)
            => RpgJson(_server.GetSheet(system, request));

        [HttpPost("{system}/settings")]
        [ProducesResponseType(typeof(RpgResponse<RpgSheetView>), StatusCodes.Status200OK)]
        public IActionResult Settings(string system, [RpgBody] RpgRequest<SheetSettings> request)
            => RpgJson(_server.Settings(system, request));

        #endregion Systems and content

        #region Describe

        [HttpPost("{system}/describe")]
        [ProducesResponseType(typeof(RpgResponse<RpgPropertyDescription>), StatusCodes.Status200OK)]
        public IActionResult Describe(string system, [RpgBody] RpgRequest<DescribeProp> request)
            => RpgJson(_server.Describe(system, request));

        [HttpPost("{system}/describe/state")]
        [ProducesResponseType(typeof(RpgResponse<RpgModSetDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeState(string system, [RpgBody] RpgRequest<DescribeState> request)
            => RpgJson(_server.Describe(system, request));

        [HttpPost("{system}/describe/modset")]
        [ProducesResponseType(typeof(RpgResponse<RpgModSetDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeModSet(string system, [RpgBody] RpgRequest<DescribeModSet> request)
            => RpgJson(_server.Describe(system, request));

        [HttpPost("{system}/describe/object")]
        [ProducesResponseType(typeof(RpgResponse<RpgObjectDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeObject(string system, [RpgBody] RpgRequest<DescribeObject> request)
            => RpgJson(_server.Describe(system, request));

        [HttpPost("{system}/describe/action")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionDescription>), StatusCodes.Status200OK)]
        public IActionResult DescribeAction(string system, [RpgBody] RpgRequest<DescribeAction> request)
            => RpgJson(_server.Describe(system, request));

        #endregion Describe

        #region Changes by hand

        [HttpPost("{system}/byhand")]
        [ProducesResponseType(typeof(RpgResponse<RpgManualChangeView>), StatusCodes.Status200OK)]
        public IActionResult ChangeByHand(string system, [RpgBody] RpgRequest<ChangeByHand> request)
            => RpgJson(_server.ChangeByHand(system, request));

        [HttpPost("{system}/byhand/undo")]
        [ProducesResponseType(typeof(RpgResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult UndoChangeByHand(string system, [RpgBody] RpgRequest<UndoChangeByHand> request)
            => RpgJson(_server.UndoChangeByHand(system, request));

        [HttpPost("{system}/state")]
        [ProducesResponseType(typeof(RpgResponse<RpgModSetDescription>), StatusCodes.Status200OK)]
        public IActionResult SetState(string system, [RpgBody] RpgRequest<SetState> request)
            => RpgJson(_server.SetState(system, request));

        #endregion Changes by hand

        #region Time

        [HttpPost("{system}/time")]
        [ProducesResponseType(typeof(RpgResponse<RpgTimeView>), StatusCodes.Status200OK)]
        public IActionResult Time(string system, [RpgBody] RpgRequest<TimeOp> request)
            => RpgJson(_server.Time(system, request));

        #endregion Time

        #region Actions

        [HttpPost("{system}/action/initiate")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult InitiateAction(string system, [RpgBody] RpgRequest<InitiateAction> request)
            => RpgJson(_server.InitiateAction(system, request));

        [HttpPost("{system}/action/step")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult ActionStep(string system, [RpgBody] RpgRequest<ActionStepRun> request)
            => RpgJson(_server.ActionStep(system, request));

        [HttpPost("{system}/action/complete")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult ActionComplete(string system, [RpgBody] RpgRequest<ActionComplete> request)
            => RpgJson(_server.ActionComplete(system, request));

        [HttpPost("{system}/action/autocomplete")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult ActionAutoComplete(string system, [RpgBody] RpgRequest<ActionAutoComplete> request)
            => RpgJson(_server.ActionAutoComplete(system, request));

        [HttpPost("{system}/action/reset")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult ActionReset(string system, [RpgBody] RpgRequest<ActionReset> request)
            => RpgJson(_server.ActionReset(system, request));

        [HttpPost("{system}/action/skippedcosts")]
        [ProducesResponseType(typeof(RpgResponse<RpgActionResult>), StatusCodes.Status200OK)]
        public IActionResult ApplySkippedCosts(string system, [RpgBody] RpgRequest<ActionComplete> request)
            => RpgJson(_server.ApplySkippedCosts(system, request));

        #endregion Actions

        #region Rolls

        [HttpPost("{system}/roll")]
        [ProducesResponseType(typeof(RpgResponse<RpgPendingRoll[]>), StatusCodes.Status200OK)]
        public IActionResult Roll(string system, [RpgBody] RpgRequest<RollOp> request)
            => RpgJson(_server.Roll(system, request));

        #endregion Rolls
    }
}
