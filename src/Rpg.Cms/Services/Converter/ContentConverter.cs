using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.States;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Converter
{
    public class ContentConverter
    {
        private readonly IEnumerable<IPropConverter> _propConverters;
        private readonly IContentTypeService _contentTypeService;

        private List<RpgObject> Entities = new();

        public void AddEntity(RpgObject entity)
        {
            if (!Entities.Any(x => x.Id == entity.Id))
                Entities.Add(entity);
        }

        public ContentConverter(IContentTypeService contentTypeService, IEnumerable<IPropConverter> propConverters)
        {
            _contentTypeService = contentTypeService;
            _propConverters = propConverters;
        }

        public RpgEntity? Convert(IMetaSystem system, Type? type, IPublishedContent source)
        {
            if (type == null)
                return null;

            var obj = (Activator.CreateInstance(type, true) as RpgObject)!;
            var target = JObject.FromObject(obj);

            target
                .AddPropIfNotNull("Id", obj.NewId())
                .AddProp("Name", source.Name)
                .AddProp("Archetype", type.Name);

            var contentType = _contentTypeService.Get(source.ContentType.Key)!;
            foreach (var propType in contentType.PropertyTypes)
            {
                var prop = source.GetProperty(propType.Alias)!;
                var fullPropName = propType.Description!;
                var propConverter = _propConverters.FirstOrDefault(x => x.CanConvert(prop));
                if (propConverter != null)
                    propConverter.Convert(system, this, target, prop, fullPropName);
            }

            var json = target.ToJson();
            var entity = RpgSerializer.Deserialize<RpgEntity>(type, json!);

            var res = entity as RpgEntity;
            if (res != null)
                AddEntity(res);

            return res;
        }
    }
}
