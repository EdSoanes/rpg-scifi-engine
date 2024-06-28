using Newtonsoft.Json.Linq;
using Rpg.Cms.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class IntegerPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Integer" &&
                (source.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.PlainInteger || source.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.Integer);

        public void Convert(JObject target, IPublishedProperty source)
        {
            var val = (source.GetValue() as int?) ?? 0;
            target.AddProp(source.Alias, val);
        }
    }
}
