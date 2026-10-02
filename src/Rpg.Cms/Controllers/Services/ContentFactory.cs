using Rpg.Cms.Extensions;
using Rpg.Cms.Services.Converter;
using Rpg.Experimental;
using Rpg.Experimental.Server;
using Rpg.Experimental.System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace Rpg.Cms.Controllers.Services
{
    /// <summary>
    /// The content library of a game system: the characters and items authored under the system's
    /// "Entity Library" node
    /// </summary>
    public class ContentFactory : IContentFactory
    {
        private readonly RpgSystems _systems;
        private readonly ContentConverter _contentConverter;
        private readonly UmbracoHelper _umbracoHelper;

        public ContentFactory(RpgSystems systems, ContentConverter contentConverter, UmbracoHelper umbracoHelper)
        {
            _systems = systems;
            _contentConverter = contentConverter;
            _umbracoHelper = umbracoHelper;
        }

        public RpgContent[] ListEntities(string systemIdentifier)
        {
            var system = _systems.Get(systemIdentifier);
            var entityLibrary = GetEntityLibrary(system);

            return entityLibrary
                .Descendants()
                .Where(x => x.ContentType.Alias != entityLibrary.ContentType.Alias)
                .Select(x => new RpgContent
                {
                    Key = x.Key,
                    Name = x.Name,
                    System = system.Identifier,
                    Archetype = system.GetArchetype(x.ContentType.Alias),
                })
                .ToArray();
        }

        public RpgObject CreateEntity(string systemIdentifier, string archetype, string contentId)
        {
            var system = _systems.Get(systemIdentifier);

            if (!RpgObjectFactory.CanCreate(system, archetype))
                throw new RpgServerException($"{archetype} cannot be created from content in system {systemIdentifier}");

            var entityLibrary = GetEntityLibrary(system);
            var alias = system.GetDocumentTypeAlias(archetype);

            var content = GetEntity(entityLibrary, alias, contentId);
            if (content == null || content.ContentType.Alias != alias)
                throw new RpgServerException($"Could not find {archetype} {contentId} in system {systemIdentifier}");

            return _contentConverter.Convert(system, content)
                ?? throw new RpgServerException($"Could not create {archetype} from content {contentId}");
        }

        private IPublishedContent GetEntityLibrary(RpgSystem system)
        {
            var systemAlias = system.GetDocumentTypeAlias(system.Identifier);
            var entityLibraryAlias = system.GetDocumentTypeAlias("Entity Library");

            var systemRoot = _umbracoHelper
                .ContentAtRoot()
                .FirstOrDefault(x => x.ContentType.Alias == systemAlias);

            var entityLibrary = systemRoot?.FirstChild(content => content.ContentType.Alias == entityLibraryAlias);
            if (entityLibrary == null)
                throw new RpgServerException($"Could not find the entity library for system {system.Identifier}. Has the system been synchronised?");

            return entityLibrary;
        }

        private IPublishedContent? GetEntity(IPublishedContent entityLibrary, string alias, string identifier)
        {
            if (Guid.TryParse(identifier, out var key))
                return _umbracoHelper.Content(key);

            //Not a key: take it as a name
            return entityLibrary
                .Descendants()
                .FirstOrDefault(x => x.ContentType.Alias == alias && string.Equals(x.Name, identifier, StringComparison.OrdinalIgnoreCase));
        }
    }
}
