﻿using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Rpg.Cms.Services.Converter
{
    public class SelectPropConverter : IPropConverter
    {
        public bool CanConvert(IPublishedProperty source)
            => source.PropertyType.EditorUiAlias == "Umb.PropertyEditorUi.Dropdown";

        public void Convert(IMetaSystem system, ContentConverter contentConverter, RpgObject target, IPublishedProperty source, string fullPropName)
        {
            //Get the data type items, then the value and return the index
            var val = (source.GetValue() as int?) ?? 0;
            target.SetProperty(fullPropName, val);
        }
    }
}
