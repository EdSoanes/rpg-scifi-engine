using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class StringPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.TinyMCE" || source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.TextBox";

        public void Convert(JObject target, IPublishedProperty source)
        {
            var val = source.GetValue()?.ToString() ?? string.Empty;
            target.AddProp(source.Alias.Replace('_', '.'), val);
        }
    }
}
