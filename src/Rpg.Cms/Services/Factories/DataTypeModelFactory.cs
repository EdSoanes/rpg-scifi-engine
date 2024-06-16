using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Factories
{
    public class DataTypeModelFactory
    {
        public CreateDataTypeRequestModel ToDataTypeModel(SyncSession session, MetaProp prop, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel();

            res.Name = session.GetDataTypeName(prop.Type);
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.EditorAlias = MapPropTypeToUmbEditor(prop.Type);
            res.EditorUiAlias = MapPropTypeToUmbUIEditor(prop.Type);
            res.Values = MapPropToUmbValues(prop);

            return res;
        }

        private string MapPropTypeToUmbEditor(string propType)
        {
            return propType switch
            {
                nameof(Int32) => "Umbraco.Integer",
                nameof(Dice) => "Umbraco.TextBox",
                "Html" => "Umbraco.RichText",
                _ => "Umbraco.TextBox"
            };
        }

        private string MapPropTypeToUmbUIEditor(string propType)
        {
            return propType switch
            {
                nameof(Int32) => "Umb.PropertyEditorUi.Integer",
                nameof(Dice) => "Umb.PropertyEditorUi.TextBox",
                "Html" => "Umb.PropertyEditorUi.TinyMCE",
                _ => "Umb.PropertyEditorUi.TextBox"
            };
        }

        private IEnumerable<DataTypePropertyPresentationModel> MapPropToUmbValues(MetaProp metaProp)
        {
            return metaProp.Type switch
            {
                nameof(Int32) => CreateIntValues(metaProp),
                "Html" => CreateHtmlValues(metaProp),
                _ => Enumerable.Empty<DataTypePropertyPresentationModel>()
            };

        }
        private IEnumerable<DataTypePropertyPresentationModel> CreateIntValues(MetaProp metaProp)
        {

            var vals = new List<DataTypePropertyPresentationModel>();
            var min = metaProp.GetValue("Min", int.MinValue);
            if (min != int.MinValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "min", Value = min });

            var max = metaProp.GetValue("Max", int.MaxValue);
            if (max != int.MaxValue)
                vals.Add(new DataTypePropertyPresentationModel { Alias = "max", Value = max });

            return vals;
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateHtmlValues(MetaProp metaProp)
        {
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
    }
}
