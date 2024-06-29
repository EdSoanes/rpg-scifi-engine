using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.Sys.Archetypes;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class ContentConverter
    {
        private readonly IEnumerable<IPropConverter> _propConverters;

        public ContentConverter(IEnumerable<IPropConverter> propConverters)
        {
            _propConverters = propConverters;
        }

        public RpgEntity? Convert(string systemIdentifier, Type type, IPublishedContent source)
        {
            var target = new JObject()
                .AddProp("id", this.NewId())
                .AddProp("name", source.Name)
                .AddProp("archetype", type.Name);

            foreach (var prop in source.Properties)
            {
                var propConverter = _propConverters.FirstOrDefault(x => x.CanConvert(prop));
                if (propConverter != null)
                    propConverter.Convert(target, prop);
                else
                    throw new InvalidOperationException($"No converter found for {prop.Alias}");
            }

            var json = target.ToJson();
            var entity = RpgSerializer.Deserialize(type, json);
            return entity as RpgEntity;
        }
    }
}
