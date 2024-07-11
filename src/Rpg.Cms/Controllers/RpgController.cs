using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Extensions;
using Rpg.Cms.Models;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.ModObjects;
using System.Security.AccessControl;
using Umbraco.Cms.Web.Common;

namespace Rpg.Cms.Controllers
{
    [ApiController]
    [Route("/api/rpg")]
    public class RpgController : Controller
    {
        private readonly ContentConverter _contentConverter;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly SyncSessionFactory _syncSessionFactory;

        public RpgController(ContentConverter contentConverter, UmbracoHelper umbracoHelper, SyncSessionFactory syncSessionFactory) 
        {
            _contentConverter = contentConverter;
            _umbracoHelper = umbracoHelper;
            _syncSessionFactory = syncSessionFactory;
        }

        [HttpGet("{system}/entities")]
        [ProducesResponseType(typeof(RpgContent[]), StatusCodes.Status200OK)]
        public IActionResult Entities(string system)
        {
            var sys = _syncSessionFactory.GetSystem(system);
            if (sys == null)
                return NotFound($"System {system} not found");

            var entityLibraryAlias = sys.GetDocumentTypeAlias("Entity Library");

            var systemRoot = _umbracoHelper.ContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == sys.Identifier);
            var entityLibrary = systemRoot?.FirstChild(content => content.ContentType.Alias == entityLibraryAlias);
            if (entityLibrary == null)
                return NotFound($"Could not find entity library {entityLibraryAlias}");

            var entities = entityLibrary.Descendants().Where(x => x.ContentType.Alias != entityLibraryAlias);

            var res = entities
                .Select(x => new RpgContent
                {
                    Key = x.Key,
                    Name = x.Name,
                    System = sys.Identifier,
                    Archetype = sys.GetArchetype(x.ContentType.Alias),
                });

            return Ok(res);
        }

        [HttpGet("{system}/{archetype}/{id}")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult Entity(string system, string archetype, Guid id)
        {
            var sys = _syncSessionFactory.GetSystem(system);
            if (sys == null)
                return NotFound($"System {system} not found");

            var type = sys.GetMetaObjectType(archetype);
            if (type == null)
                return BadRequest($"No .net type found for archetype {archetype} in system {system}");

            var content = _umbracoHelper.Content(id);
            if (!sys.IsContentForSystem(content))
                return NotFound($"Entity of archetype {archetype} in system {system} not found");

            var entity = (_contentConverter.Convert(sys, type, content!) as RpgObject)!;
            var graph = new RpgGraph(entity);

            var graphStateJson = graph.Serialize();
            return Content(graphStateJson, "application/json");
        }
    }
}
