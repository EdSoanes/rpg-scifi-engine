using Rpg.Cms.Extensions;
using Rpg.Cms.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Server.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class ContainerPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.MultiNodeTreePicker;

        public void Convert(IMetaSystem system, ContentConverter contentConverter, RpgObject target, IPublishedProperty source, string fullPropName)
        {
            var items = source.GetValue() as IEnumerable<IPublishedContent>;
            if (items != null)
            {
                var entities = new List<RpgObject>();
                foreach (var item in items)
                {
                    var itemType = system.GetMetaObjectType(item);
                    var entity = contentConverter.Convert(system, itemType, item);
                    if (entity != null)
                    { 
                        contentConverter.AddEntity(entity);
                        entities.Add(entity);
                    }
                }

                if (entities.Any())
                {
                    var container = target.GetProperty<RpgContainer>(fullPropName) ?? new RpgContainer(source.Alias);
                    foreach (var entity in entities.Where(x => x is RpgEntity))
                        container.Add((RpgEntity)entity);

                    target.SetProperty(fullPropName, container);
                }
            }
        }
    }
}
