using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class BooleanPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Toggle";

        public void Convert(JObject target, IPublishedProperty source)
        {
            var val = (source.GetValue() as int?) ?? 0;
            target.AddProp(source.Alias.Replace('_', '.'), val > 0);
        }
    }
}
