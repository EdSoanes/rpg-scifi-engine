using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Converter
{
    public class ContentConverter
    {
        private readonly IEnumerable<IPropConverter> _propConverters;
        private readonly IContentTypeService _contentTypeService;

        private List<RpgEntity> Entities = new();

        public void AddEntity(RpgEntity entity)
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

            var obj = (Activator.CreateInstance(type, true) as RpgEntity)!;
            obj.SetProperty("Name", source.Name);
            obj.SetProperty("Archetype", type.Name);

            var contentType = _contentTypeService.Get(source.ContentType.Key)!;
            foreach (var propType in contentType.PropertyTypes)
            {
                var prop = source.GetProperty(propType.Alias)!;
                var fullPropName = propType.Description!;
                var propConverter = _propConverters.FirstOrDefault(x => x.CanConvert(prop));
                if (propConverter != null)
                    propConverter.Convert(system, this, obj, prop, fullPropName);
            }

            AddEntity(obj);

            return obj;
        }
    }
}
