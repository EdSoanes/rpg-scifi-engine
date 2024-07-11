using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class StringPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.TinyMCE" || source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.TextBox";

        public void Convert(IMetaSystem system, ContentConverter contentConverter, JObject target, IPublishedProperty source, string fullPropName)
        {
            var val = source.GetValue()?.ToString() ?? string.Empty;
            target.AddProp(fullPropName, val);
        }
    }
}
