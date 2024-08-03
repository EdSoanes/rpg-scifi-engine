using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Extensions
{
    public static class MetaSystemExtensions
    {
        public static string GetArchetype(this IMetaSystem system, string alias)
        {
            var archetype = alias.StartsWith(system.Identifier)
                ? alias.Substring(system.Identifier.Length)
                : alias;

            return archetype.Replace("_", "").Trim();
        }

        public static string GetDocumentTypeAlias(this IMetaSystem system, string archetype)
            => !archetype.StartsWith(system.Identifier)
                ? $"{system.Identifier}_{archetype}".Replace(" ", "")
                : archetype;

        public static Type? GetMetaObjectType(this IMetaSystem system, IPublishedContent publishedContent)
        {
            var archetype = publishedContent.ContentType.Alias.StartsWith(system.Identifier)
                ? publishedContent.ContentType.Alias.Substring(system.Identifier.Length)
                : publishedContent.ContentType.Alias;

            archetype = archetype.Replace("_", "").Trim();

            return system.GetMetaObjectType(archetype);
        }

        public static Type? GetMetaObjectType(this IMetaSystem system, string archetype)
        {
            var metaObj = system.Objects.FirstOrDefault(x => x.Archetype == archetype);
            if (metaObj != null)
                return !string.IsNullOrEmpty(metaObj.QualifiedClassName)
                    ? RpgTypeScan.ForType(metaObj.QualifiedClassName)
                    : RpgTypeScan.ForTypeByName(metaObj.Archetype);

            return null;
        }

        public static bool IsContentForSystem(this IMetaSystem system, IPublishedContent? publishedContent)
            => publishedContent?.ContentType.Alias.StartsWith(system.Identifier) ?? false;
    }
}
