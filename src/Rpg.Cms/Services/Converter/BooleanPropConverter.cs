using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class BooleanPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Toggle";

        public void Convert(IMetaSystem system, ContentConverter contentConverter, JObject target, IPublishedProperty source, string fullPropName)
        {
            var val = (source.GetValue() as int?) ?? 0;
            target.AddProp(fullPropName, val > 0);
        }
    }
}
