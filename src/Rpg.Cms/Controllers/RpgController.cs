using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Rpg.Cms.Extensions;
using Rpg.Cms.Models;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Props;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace Rpg.Cms.Controllers
{
    [Route("api/v{version:apiVersion}/rpg")]
    [ApiController]
    [ApiVersion("1.0")]
    [MapToApi("rpg")]
    [ApiExplorerSettings(GroupName = "Entities")]
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

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/entities")]
        [ProducesResponseType(typeof(RpgContent[]), StatusCodes.Status200OK)]
        public IActionResult Entities(string system)
        {
            var sys = _syncSessionFactory.GetSystem(system);
            if (sys == null)
                return NotFound($"System {system} not found");

            var entityLibrary = GetEntityLibrary(sys);
            if (entityLibrary == null)
                return NotFound($"Could not find entity library for system {system}");

            var entities = entityLibrary
                .Descendants()
                .Where(x => x.ContentType.Alias != entityLibrary.ContentType.Alias);

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

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpPost("{system}/state")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult StateState(string system, RpgOperation<SetState> setStateOperation)
        {
            var graph = new RpgGraph(setStateOperation.GraphState);
            var entity = graph.GetObject<RpgEntity>(setStateOperation.Operation.EntityId)!;
            var stateChanged = setStateOperation.Operation.On
                ? entity.SetStateOn(setStateOperation.Operation.State)
                : entity.SetStateOff(setStateOperation.Operation.State);

            graph.Time.TriggerEvent();

            var graphState = graph.GetGraphState();
            return Ok(graphState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// POST {system}/describe
        /// </remarks>
        /// <param name="system"></param>
        /// <param name="describeOperation"></param>
        /// <returns></returns>
        [HttpPost("{system}/describe")]
        [ProducesResponseType(typeof(PropDesc), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Describe(string system, RpgOperation<Describe> describeOperation)
        {
            var graph = new RpgGraph(describeOperation.GraphState);
            graph.Time.TriggerEvent();

            var entity = graph.GetObject<RpgEntity>(describeOperation.Operation.EntityId)!;

            var description = entity.Describe(graph, describeOperation.Operation.Prop);
            if (description == null)
                return BadRequest($"Desription for {describeOperation.Operation.EntityId}.{describeOperation.Operation.Prop} not found");

            return Ok(description);
        }

        [EnableCors(CorsComposer.AllowAnyOriginPolicyName)]
        [HttpGet("{system}/{archetype}/{id}")]
        [ProducesResponseType(typeof(RpgGraphState), StatusCodes.Status200OK)]
        public IActionResult Entity(string system, string archetype, string id)
        {
            var sys = _syncSessionFactory.GetSystem(system);
            if (sys == null)
                return NotFound($"System {system} not found");

            var type = sys.GetMetaObjectType(archetype);
            if (type == null)
                return BadRequest($"No .net type found for archetype {archetype} in system {system}");

            var entityLibrary = GetEntityLibrary(sys);
            if (entityLibrary == null)
                return NotFound($"Could not find entity library for system {system}");

            var alias = sys.GetDocumentTypeAlias(archetype);
            var content = GetEntity(entityLibrary, alias, id);
            if (content == null)
                return NotFound($"Could not find entity {id} ({archetype}) for system {system}");

            var entity = _contentConverter.Convert(sys, type, content!);
            var graph = new RpgGraph(entity!);

            var graphState = graph.GetGraphState();

            var json = RpgSerializer.Serialize(graphState);
            return Content(json, "application/json");
        }

        private IPublishedContent? GetEntityLibrary(IMetaSystem system)
        {
            var entityLibraryAlias = system.GetDocumentTypeAlias("Entity Library");

            var systemRoot = _umbracoHelper
                .ContentAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == system.Identifier);

            var entityLibrary = systemRoot?.FirstChild(content => content.ContentType.Alias == entityLibraryAlias);
            return entityLibrary;
        }

        private IPublishedContent? GetEntity(IPublishedContent entityLibrary, string alias, string identifier)
        {
            if (Guid.TryParse(identifier, out var key))
                return _umbracoHelper.Content(key);

            var entity = entityLibrary
                .Descendants()
                .FirstOrDefault(x => x.ContentType.Alias == alias);

            return entity;
        }
    }
}
