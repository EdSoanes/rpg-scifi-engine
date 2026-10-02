using Rpg.Cms.Services;
using Rpg.Experimental;
using Rpg.Experimental.System;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Extensions
{
    public static class RpgSystemExtensions
    {
        /// <summary>
        /// The archetype a document type alias stands for, e.g. Cyborgs_MeleeWeapon is MeleeWeapon
        /// </summary>
        public static string GetArchetype(this RpgSystem system, string alias)
        {
            var archetype = alias.StartsWith(system.Identifier)
                ? alias.Substring(system.Identifier.Length)
                : alias;

            return archetype.Replace("_", "").Trim();
        }

        public static string GetDocumentTypeAlias(this RpgSystem system, string archetype)
            => !archetype.StartsWith(system.Identifier)
                ? $"{system.Identifier}_{archetype}".Replace(" ", "")
                : archetype;

        public static bool IsContentForSystem(this RpgSystem system, IPublishedContent? publishedContent)
            => publishedContent?.ContentType.Alias.StartsWith(system.Identifier) ?? false;

        /// <summary>
        /// The objects of the system that an author can create: those with a template
        /// </summary>
        public static MetaObject[] AuthorableObjects(this RpgSystem system)
            => system.Objects
                .Where(x => x.Template != null)
                .ToArray();

        /// <summary>
        /// The document type for a game system object. Its properties are the values of the object's
        /// template (the name is the name of the content itself), plus a picker for each property that
        /// holds other objects, e.g. the items in a character's hands.
        /// </summary>
        public static DocTypeTemplate AsDocTypeTemplate(this RpgSystem system, MetaObject metaObject)
        {
            var template = new DocTypeTemplate(metaObject.Archetype)
                .AddIcon(metaObject.Icon ?? "icon-checkbox-dotted");

            foreach (var prop in metaObject.Template?.Properties ?? [])
            {
                if (prop.Prop == nameof(RpgObject.Name))
                    continue;

                template.AddProp(prop.Prop, RpgDataTypes.For(prop.Editor), prop.DisplayName, prop.Tab, prop.Group);
            }

            foreach (var prop in metaObject.Properties.Where(x => x.PropertyType == RpgPropertyType.Children && !x.Path.Any()))
                template.AddProp(prop.Prop, RpgDataTypes.Children, prop.DisplayName, prop.Tab, prop.Group);

            return template;
        }
    }
}
