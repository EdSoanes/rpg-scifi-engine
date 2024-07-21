using Rpg.Cms.Extensions;
using Rpg.Cms.Models;
using Rpg.Cms.Services.Converter;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using System.Net;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace Rpg.Cms.Controllers.Services
{
    public class ContentFactory : IContentFactory
    {
        private readonly ContentConverter _contentConverter;
        private readonly UmbracoHelper _umbracoHelper;

        public ContentFactory(ContentConverter contentConverter, UmbracoHelper umbracoHelper)
        {
            _contentConverter = contentConverter;
            _umbracoHelper = umbracoHelper;
        }

        public RpgContent[] ListEntities(string systemIdentifier)
        {
            var system = MetaSystems.Get(systemIdentifier);
            if (system == null)
                throw new HttpRequestException($"System {system} not found", null, HttpStatusCode.BadRequest);

            var entityLibrary = GetEntityLibrary(system);
            var entities = entityLibrary
                .Descendants()
                .Where(x => x.ContentType.Alias != entityLibrary.ContentType.Alias);

            var res = entities
                .Select(x => new RpgContent
                {
                    Key = x.Key,
                    Name = x.Name,
                    System = system.Identifier,
                    Archetype = system.GetArchetype(x.ContentType.Alias),
                })
                .ToArray();

            return res;
        }

        public RpgEntity GetEntity(string systemIdentifier, string archetype, string id)
        {
            var system = MetaSystems.Get(systemIdentifier);
            if (system == null)
                throw new HttpRequestException($"System {system} not found", null, HttpStatusCode.BadRequest);

            var entityLibrary = GetEntityLibrary(system);

            var type = system.GetMetaObjectType(archetype);
            if (type == null)
                throw new HttpRequestException($"No .net type found for archetype {archetype} in system {systemIdentifier}", null, HttpStatusCode.BadRequest);

            var alias = system.GetDocumentTypeAlias(archetype);
            var content = GetEntity(entityLibrary, alias, id);
            if (content == null)
                throw new HttpRequestException($"Could not find entity {id} ({archetype}) for system {systemIdentifier}", null, HttpStatusCode.BadRequest);

            var entity = _contentConverter.Convert(system, type, content)!;
            return entity;
        }

        private IPublishedContent GetEntityLibrary(IMetaSystem system)
        {
            var entityLibraryAlias = system.GetDocumentTypeAlias("Entity Library");

            var systemRoot = _umbracoHelper
                .ContentAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == system.Identifier);

            var entityLibrary = systemRoot?.FirstChild(content => content.ContentType.Alias == entityLibraryAlias);
            if (entityLibrary == null)
                throw new HttpRequestException($"Could not find entity library for system {system.Identifier}", null, HttpStatusCode.BadRequest);

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
