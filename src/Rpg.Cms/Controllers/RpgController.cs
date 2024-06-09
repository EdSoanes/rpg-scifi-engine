using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Services;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Rpg.Cms.Controllers
{
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
    [ApiExplorerSettings(GroupName = "_Rpg")]
    public class RpgController : ManagementApiControllerBase
    {
        private readonly IRpgSystemSyncService _rpgSystemSyncService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public RpgController(IRpgSystemSyncService rpgSystemSyncService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor) 
        {
            _rpgSystemSyncService = rpgSystemSyncService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        [HttpPost("sync")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync()
        {
            await _rpgSystemSyncService.Sync(CurrentUserKey(_backOfficeSecurityAccessor));
            return Ok();
        }

        [HttpGet("meta")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
        public IActionResult Meta()
        {
            var meta = new MetaGraph();
            var metaSystem = meta.Build();

            return Ok(metaSystem);
        }
    }
}
