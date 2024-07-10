using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Rpg.Cms.Extensions
{
    public static class MetaPropAttributeExtensions
    {
        public static string UmbEditor(this MetaPropAttribute attr)
        {
            return attr.Editor switch
            {
                EditorType.Int32 => "Umbraco.Integer",
                EditorType.Select => "Umbraco.DropDown.Flexible",
                EditorType.Boolean => "Umbraco.TrueFalse",
                EditorType.CheckBoxList => "Umbraco.CheckBoxList",
                EditorType.RichText => "Umbraco.RichText",
                _ => "Umbraco.Text"
            };
        }

        public static string UmbUIEditor(this MetaPropAttribute attr)
        {
            return attr.Editor switch
            {
                EditorType.Boolean => "Umb.PropertyEditorUi.Toggle",
                EditorType.Select => "Umb.PropertyEditorUi.Dropdown",
                EditorType.CheckBoxList => "Umb.PropertyEditorUi.CheckBoxList",
                EditorType.RichText => "Umb.PropertyEditorUi.TinyMCE",
                EditorType.Text => "Umb.PropertyEditorUi.TextBox",
                _ => "Umb.PropertyEditorUi.Integer",
            };
        }

        public static IEnumerable<DataTypePropertyPresentationModel> UmbDataTypeValues(this MetaPropAttribute attr)
        {
            var vals = attr.Editor switch
            {
                EditorType.Int32 => attr.UmbInt32Values(),
                EditorType.Select => attr.UmbMultiOptionValues(),
                EditorType.CheckBoxList => attr.UmbMultiOptionValues(),
                EditorType.RichText => attr.UmbRichTextValues(),
                _ => null
            };

            return vals?
                .Where(x => x.Value != null)
                .Select(x => new DataTypePropertyPresentationModel { Alias = x.Key, Value = x.Value })
                ?? Enumerable.Empty<DataTypePropertyPresentationModel>();
        }

        private static Dictionary<string, object> UmbInt32Values(this MetaPropAttribute attr)
        { 
            var res = new Dictionary<string, object>();

            var max = attr.Value("Max", int.MaxValue);
            if (max != int.MaxValue)
                res.Add("max", max);

            var min = attr.Value("Min", int.MinValue);
            if (min != int.MinValue)
                res.Add("min", min);

            return res;
        }

        private static Dictionary<string, object> UmbMultiOptionValues(this MetaPropAttribute attr)
        {
            var res = new Dictionary<string, object>();

            var values = attr.Value("Values", Array.Empty<string>());
            if (values.Any())
                res.Add("items", values!);

            return res;
        }

        private static Dictionary<string, object> UmbRichTextValues(this MetaPropAttribute attr)
        {
            var res = new Dictionary<string, object>();
            res.Add("mode", "classic");
            res.Add("toolbar", new object[]
            {
                "cut",
                "copy",
                "paste",
                "bold",
                "italic",
                "alignleft",
                "aligncenter",
                "alignright",
                "alignjustify",
                "bullist",
                "numlist",
                "outdent",
                "indent",
                "sourcecode"
            });

            return res;
        }
    }
}
