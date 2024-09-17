using Rpg.Cms.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class IntegerPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Integer" &&
                (source.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.PlainInteger || source.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.Integer);

        public void Convert(IMetaSystem system, ContentConverter contentConverter, RpgObject target, IPublishedProperty source, string fullPropName)
        {
            var val = (source.GetValue() as int?) ?? 0;
            target.SetProperty(fullPropName, val);
        }
    }
}
