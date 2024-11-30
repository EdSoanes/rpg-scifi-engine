//using Asp.Versioning;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Rpg.Cms.Services;
//using Rpg.Cms.Services.Converter;
//using Rpg.ModObjects.Meta;
//using Rpg.ModObjects.Server.Json;
//using Umbraco.Cms.Api.Common.Attributes;
//using Umbraco.Cms.Api.Management.Controllers;
//using Umbraco.Cms.Core.Mapping;
//using Umbraco.Cms.Core.Security;
//using Umbraco.Cms.Web.Common;
//using Umbraco.Cms.Web.Common.Authorization;

//namespace Rpg.Cms.Controllers
//{
//    [Route("api/v{version:apiVersion}/rpgadmin")]
//    [ApiController]
//    [ApiVersion("1.0")]
//    [MapToApi("rpgadmin")]
//    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
//    [ApiExplorerSettings(GroupName = "Management")]
//    public class RpgManagementController : ManagementApiControllerBase
//    {
//        private readonly ContentConverter _contentConverter;
//        private readonly SyncSessionFactory _syncSessionFactory;
//        private readonly ISyncTypesService _syncTypesService;
//        private readonly ISyncContentService _syncContentService;
//        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
//        private readonly IUmbracoMapper _umbracoMapper;
//        private readonly UmbracoHelper _umbracoHelper;

//        public RpgManagementController(
//            ContentConverter contentConverter,
//            SyncSessionFactory syncSessionFactory, 
//            ISyncTypesService syncTypesService,
//            ISyncContentService syncContentService,
//            IBackOfficeSecurityAccessor backOfficeSecurityAccessor, 
//            IUmbracoMapper umbracoMapper,
//            UmbracoHelper umbracoHelper) 
//        {
//            _contentConverter = contentConverter;
//            _syncSessionFactory = syncSessionFactory;
//            _syncTypesService = syncTypesService;
//            _syncContentService = syncContentService;
//            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
//            _umbracoMapper = umbracoMapper;
//            _umbracoHelper = umbracoHelper;
//        }

//        [HttpGet("systems")]
//        [MapToApiVersion("1.0")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> Systems()
//        {
//            var systems = MetaSystems.Get();
//            var json = RpgJson.Serialize(systems);

//            return Content(json, "application/json");
//        }

//        [HttpPost("sync")]
//        [MapToApiVersion("1.0")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        public async Task<IActionResult> Sync()
//        {
//            var userKey = CurrentUserKey(_backOfficeSecurityAccessor);

//            var systems = MetaSystems.Get();
//            foreach (var system in systems)
//            {
//                var session = _syncSessionFactory.CreateSession(userKey, system);

//                await _syncTypesService.Sync(session);
//                await _syncContentService.Sync(session);
//            }

//            return Ok();
//        }

//        [HttpPost("sync/{identifier}")]
//        [MapToApiVersion("1.0")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> Sync(string identifier)
//        {
//            var userKey = CurrentUserKey(_backOfficeSecurityAccessor);

//            var system = MetaSystems.Get(identifier);
//            if (system == null)
//                return NotFound();

//            var session = _syncSessionFactory.CreateSession(userKey, system);

//            await _syncTypesService.Sync(session);
//            await _syncContentService.Sync(session);

//            return Ok();
//        }

//        [HttpGet("meta")]
//        [MapToApiVersion("1.0")]
//        [ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
//        public IActionResult Meta()
//        {
//            var meta = new MetaGraph();
//            var metaSystem = meta.Build();

//            var json = RpgJson.Serialize(metaSystem);
//            return Content(json, "application/json");
//        }

//        //[HttpGet("document-types")]
//        //[MapToApiVersion("1.0")]
//        //[ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
//        //public IActionResult DocumentTypes()
//        //{
//        //    var docTypes = _syncTypesService.DocumentTypes();

//        //    IEnumerable<DocumentTypeResponseModel> models = docTypes.Select(x => _umbracoMapper.Map<DocumentTypeResponseModel>(x)!);
//        //    return Ok(models);
//        //}

//        //[HttpGet("document-type-updates")]
//        //[MapToApiVersion("1.0")]
//        //[ProducesResponseType(typeof(IMetaSystem), StatusCodes.Status200OK)]
//        //public async Task<IActionResult> DocumentTypeUpdates()
//        //{
//        //    var res = await _syncTypesService.DocumentTypeUpdatesAsync(CurrentUserKey(_backOfficeSecurityAccessor));

//        //    var json = RpgJson.Serialize(res);
//        //    return Content(json, "application/json");
//        //}
//    }
//}
