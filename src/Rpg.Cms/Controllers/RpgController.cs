using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys;
using Rpg.Sys.Archetypes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Authorization;

namespace Rpg.Cms.Controllers
{
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
    [ApiExplorerSettings(GroupName = "_Rpg")]
    public class RpgController : ManagementApiControllerBase
    {
        private readonly ContentConverter _contentConverter;
        private readonly SyncSessionFactory _syncSessionFactory;
        private readonly ISyncTypesService _syncTypesService;
        private readonly ISyncContentService _syncContentService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly UmbracoHelper _umbracoHelper;

        public RpgController(
            ContentConverter contentConverter,
            SyncSessionFactory syncSessionFactory, 
            ISyncTypesService syncTypesService,
            ISyncContentService syncContentService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor, 
            IUmbracoMapper umbracoMapper,
            UmbracoHelper umbracoHelper) 
        {
            _contentConverter = contentConverter;
            _syncSessionFactory = syncSessionFactory;
            _syncTypesService = syncTypesService;
            _syncContentService = syncContentService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _umbracoMapper = umbracoMapper;
            _umbracoHelper = umbracoHelper;
        }

        [HttpPost("sync")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync()
        {
            var userKey = CurrentUserKey(_backOfficeSecurityAccessor);

            var systems = _syncSessionFactory.GetSystems();
            var session = _syncSessionFactory.CreateSession(userKey, systems.First());

            await _syncTypesService.Sync(session);
            await _syncContentService.Sync(session);

            return Ok();
        }

        [HttpGet("entity/{system}/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<DocumentTypeResponseModel>), StatusCodes.Status200OK)]
        public IActionResult Entity(string system, Guid id)
        {
            var content = _umbracoHelper.Content(id);
            if (content == null || !content.ContentType.Alias.StartsWith(system))
                return NotFound();

            var entity = _contentConverter.Convert<Human>(content)!;
            var graph = new RpgGraph(entity);

            var json = RpgSerializer.Serialize(entity);

            return Content(json, "application/json");
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
            var docTypes = _syncTypesService.DocumentTypes();

            IEnumerable<DocumentTypeResponseModel> models = docTypes.Select(x => _umbracoMapper.Map<DocumentTypeResponseModel>(x)!);
            return Ok(models);
        }

        [HttpGet("document-type-updates")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
        public async Task<IActionResult> DocumentTypeUpdates()
        {
            var res = await _syncTypesService.DocumentTypeUpdatesAsync(CurrentUserKey(_backOfficeSecurityAccessor));

            var json = RpgSerializer.Serialize(res);
            return Content(json, "application/json");
        }
    }
}
