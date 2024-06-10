using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Services;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
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
        private readonly IUmbracoMapper _umbracoMapper;

        public RpgController(IRpgSystemSyncService rpgSystemSyncService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IUmbracoMapper umbracoMapper) 
        {
            _rpgSystemSyncService = rpgSystemSyncService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _umbracoMapper = umbracoMapper;
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
        [ProducesResponseType(typeof(IEnumerable<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
        public IActionResult Meta()
        {
            var meta = new MetaGraph();
            var metaSystem = meta.Build();

            var json = RpgSerializer.Serialize(metaSystem);
            return Content(json, "application/json");
        }

        [HttpGet("document-types")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
        public IActionResult DocumentTypes()
        {
            var docTypes = _rpgSystemSyncService.DocumentTypes();

            IEnumerable<DocumentTypeResponseModel> models = docTypes.Select(x => _umbracoMapper.Map<DocumentTypeResponseModel>(x)!);
            return Ok(models);
        }

        [HttpGet("document-type-updates")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
        public async Task<IActionResult> DocumentTypeUpdates()
        {
            var res = await _rpgSystemSyncService.DocumentTypeUpdatesAsync(CurrentUserKey(_backOfficeSecurityAccessor));

            var json = RpgSerializer.Serialize(res);
            return Content(json, "application/json");
        }
    }
}
