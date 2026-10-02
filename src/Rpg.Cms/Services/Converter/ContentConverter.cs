using Rpg.Cms.Extensions;
using Rpg.Experimental;
using Rpg.Experimental.System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Converter
{
    /// <summary>
    /// Turns authored content into a game system object: the content's values fill the object's template,
    /// and the content picked into its child properties (e.g. the items in a character's hands) is turned
    /// into objects too.
    /// </summary>
    public class ContentConverter
    {
        /// <summary>Content can pick content that picks it back. This is where following that stops.</summary>
        private const int MaxDepth = 8;

        private readonly IContentTypeService _contentTypeService;

        public ContentConverter(IContentTypeService contentTypeService)
            => _contentTypeService = contentTypeService;

        public RpgObject? Convert(RpgSystem system, IPublishedContent source)
            => Convert(system, source, 0);

        private RpgObject? Convert(RpgSystem system, IPublishedContent source, int depth)
        {
            var archetype = system.GetArchetype(source.ContentType.Alias);
            if (depth > MaxDepth || !RpgObjectFactory.CanCreate(system, archetype))
                return null;

            var propNames = PropNamesByAlias(source);

            var values = new Dictionary<string, object?>();
            var children = new List<(string, IPublishedContent)>();

            foreach (var property in source.Properties)
            {
                if (!propNames.TryGetValue(property.Alias, out var propName))
                    continue;

                var value = property.GetValue();
                switch (value)
                {
                    case IPublishedContent picked:
                        children.Add((propName, picked));
                        break;

                    case IEnumerable<IPublishedContent> pickedItems:
                        children.AddRange(pickedItems.Select(x => (propName, x)));
                        break;

                    default:
                        values[propName] = value;
                        break;
                }
            }

            values[nameof(RpgObject.Name)] = source.Name;

            var obj = RpgObjectFactory.Create(system, archetype, values);

            foreach (var (propName, picked) in children)
            {
                var child = Convert(system, picked, depth + 1);
                if (child != null)
                    RpgObjectFactory.AddChild(obj, propName, child);
            }

            return obj;
        }

        /// <summary>
        /// The description of a property type holds the name of the property on the game system object
        /// (see DocTypeModelFactory). The alias is only a safe form of it.
        /// </summary>
        private Dictionary<string, string> PropNamesByAlias(IPublishedContent source)
        {
            var res = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var contentType = _contentTypeService.Get(source.ContentType.Key);
            if (contentType != null)
                foreach (var propType in contentType.PropertyTypes)
                    res[propType.Alias] = !string.IsNullOrEmpty(propType.Description) ? propType.Description : propType.Alias;

            return res;
        }
    }
}
