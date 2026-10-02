using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Services;
using Rpg.Experimental.Server;
using Rpg.Experimental.System;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Security;

namespace Rpg.Cms.Controllers
{
    /// <summary>
    /// For someone signed in to the backoffice: bring the document types and library content into line with
    /// the game systems.
    /// </summary>
    [ApiVersion("1.0")]
    [VersionedApiBackOfficeRoute("rpg")]
    [ApiExplorerSettings(GroupName = "Rpg")]
    [MapToApi(RpgOpenApi.ManagementDocument)]
    public class RpgManagementController : ManagementApiControllerBase
    {
        private readonly RpgSystems _systems;
        private readonly RpgSyncService _syncService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public RpgManagementController(
            RpgSystems systems,
            RpgSyncService syncService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _systems = systems;
            _syncService = syncService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        [HttpGet("systems")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public IActionResult Systems()
            => Ok(_systems.All().Select(x => x.Identifier).ToArray());

        [HttpPost("sync")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync()
        {
            var userKey = CurrentUserKey(_backOfficeSecurityAccessor);

            var synced = new List<string>();
            foreach (var system in _systems.All())
            {
                await _syncService.Sync(system, userKey);
                synced.Add(system.Identifier);
            }

            return Ok(synced.ToArray());
        }

        [HttpPost("sync/{identifier}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Sync(string identifier)
        {
            var system = _systems.Find(identifier);
            if (system == null)
                return NotFound();

            await _syncService.Sync(system, CurrentUserKey(_backOfficeSecurityAccessor));

            return Ok(new[] { system.Identifier });
        }
    }
}
