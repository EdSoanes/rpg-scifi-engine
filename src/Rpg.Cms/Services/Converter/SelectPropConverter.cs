using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class SelectPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Dropdown";

        public void Convert(JObject target, IPublishedProperty source)
        {
            //Get the data type items, then the value and return the index
            var val = (source.GetValue() as int?) ?? 0;
            target.AddProp(source.Alias.Replace('_', '.'), val);
        }
    }
}
