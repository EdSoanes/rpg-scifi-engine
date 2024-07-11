using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Extensions
{
    public static class MetaSystemExtensions
    {
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
                    ? RpgReflection.ScanForType(metaObj.QualifiedClassName)
                    : RpgReflection.ScanForTypeByName(metaObj.Archetype);

            return null;
        }

        public static bool IsContentForSystem(this IMetaSystem system, IPublishedContent? publishedContent)
            => publishedContent?.ContentType.Alias.StartsWith(system.Identifier) ?? false;
    }
}
