using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public class RpgDataTypeFactory : IRpgDataTypeFactory
    {
        public string GetName(IMetaSystem system, MetaPropUIAttribute propUIAttribute)
            => $"{system.Identifier} {propUIAttribute.Name}".Replace("UIAttribute", "");

        public CreateDataTypeRequestModel Create(IMetaSystem system, IUmbracoEntity parentFolder, MetaPropUIAttribute propUIAttribute)
        {
            var res = new CreateDataTypeRequestModel();

            res.Name = GetName(system, propUIAttribute);
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.EditorAlias = MapEditorToUmbEditor(propUIAttribute.Editor);
            res.EditorUiAlias = MapEditorToUmbUIEditor(propUIAttribute.Editor);
            res.Values = propUIAttribute.Editor switch
            {
                nameof(Int32) => CreateIntValues(propUIAttribute),
                "Html" => CreateHtmlValues(propUIAttribute),
                _ => Enumerable.Empty<DataTypePropertyPresentationModel>()
            };

            return res;
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateIntValues(MetaPropUIAttribute propUIAttribute)
        {
            var attr = propUIAttribute as IntegerUIAttribute;
            if (attr == null)
                return Enumerable.Empty<DataTypePropertyPresentationModel>();

            var vals = new List<DataTypePropertyPresentationModel>();
            if (attr.Min != int.MinValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "min", Value = attr.Min });
            if (attr.Max != int.MaxValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "max", Value = attr.Max });

            return vals;
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateHtmlValues(MetaPropUIAttribute propUIAttribute)
        {
            var attr = propUIAttribute as RichTextUIAttribute;
            if (attr == null)
                return Enumerable.Empty<DataTypePropertyPresentationModel>();

            var vals = new List<DataTypePropertyPresentationModel>
            {
                new DataTypePropertyPresentationModel { Alias = "mode", Value = "classic" },
                new DataTypePropertyPresentationModel { Alias = "toolbar", Value = new object[]
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
                }}
            };

            return vals;
        }

        private string MapEditorToUmbEditor(string editor)
        {
            return editor switch
            {
                nameof(Int32) => "Umbraco.Integer",
                nameof(Dice) => "Umbraco.TextBox",
                "Html" => "Umbraco.RichText",
                _ => "Umbraco.TextBox"
            };
        }

        private string MapEditorToUmbUIEditor(string editor)
        {
            return editor switch
            {
                nameof(Int32) => "Umb.PropertyEditorUi.Integer",
                nameof(Dice) => "Umb.PropertyEditorUi.TextBox",
                "Html" => "Umb.PropertyEditorUi.TinyMCE",
                _ => "Umb.PropertyEditorUi.TextBox"
            };
        }
    }
}
