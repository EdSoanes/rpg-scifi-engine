using Humanizer;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Values;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Factories
{
    public class DataTypeModelFactory
    {
        public CreateDataTypeRequestModel[] ToDataTypeModels(SyncSession session, IUmbracoEntity parentFolder)
        {
            var res = new List<CreateDataTypeRequestModel>();
            foreach (var attr in session.System.PropUIs)
            {
                var dataTypeName = session.GetDataTypeName(attr.DataType);
                if (!res.Any(x => x.Name == dataTypeName))
                {
                    var model = ToDataTypeModel(session, attr, parentFolder);
                    res.Add(model);
                }
            }

            foreach (var metaObj in session.System.Objects)
            {
                var actionDataType = ToActionDataTypeModel(session, metaObj, parentFolder);
                var stateDataType = ToStateDataTypeModel(session, metaObj, parentFolder);

                if (actionDataType != null)
                    res.Add(actionDataType);

                if (stateDataType != null)
                    res.Add(stateDataType);
            }

            return res.ToArray();
        }

        public CreateDataTypeRequestModel? ToActionDataTypeModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
            => ToCheckBoxList(session, metaObj, parentFolder, "Actions", metaObj.AllowedActions.Select(x => $"{session.System.Identifier}.{x}").ToArray());

        public CreateDataTypeRequestModel? ToStateDataTypeModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
            => ToCheckBoxList(session, metaObj, parentFolder, "States", metaObj.AllowedStates.Select(x => $"{session.System.Identifier}.{x}").ToArray());

        public CreateDataTypeRequestModel ToDataTypeModel(SyncSession session, MetaPropUIAttribute attr, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel();

            res.Id = Guid.NewGuid();
            res.Name = session.GetDataTypeName(attr.DataType);
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.EditorAlias = ToUmbEditor(attr);
            res.EditorUiAlias = ToUmbUIEditor(attr);
            res.Values = ToUmbValues(attr);

            return res;
        }

        private CreateDataTypeRequestModel? ToCheckBoxList(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder, string name, string[] items)
        {
            if (metaObj.AllowedActions.Any())
            {
                var res = new CreateDataTypeRequestModel();

                res.Id = Guid.NewGuid();
                res.Name = session.GetDataTypeName($"{metaObj.Archetype} {name}");
                res.Parent = new ReferenceByIdModel(parentFolder.Key);
                res.EditorAlias = "Umbraco.CheckBoxList";
                res.EditorUiAlias = "Umb.PropertyEditorUi.CheckBoxList";

                res.Values = new List<DataTypePropertyPresentationModel>
                {
                    new DataTypePropertyPresentationModel { Alias = "items", Value = items }
                };

                return res;
            }

            return null;
        }
        private string ToUmbEditor(MetaPropUIAttribute attr)
        {
            return attr.ReturnType switch
            {
                nameof(Int32) => "Umbraco.Integer",
                nameof(Dice) => "Umbraco.TextBox",
                _ => attr.DataType == "RichText" 
                    ? "Umbraco.RichText" 
                    : "Umbraco.TextBox"
            };
        }

        private string ToUmbUIEditor(MetaPropUIAttribute attr)
        {
            return attr.DataType switch
            {
                nameof(Dice) => "Umb.PropertyEditorUi.TextBox",
                "RichText" => "Umb.PropertyEditorUi.TinyMCE",
                "Text" => "Umb.PropertyEditorUi.TextBox",
                _ => "Umb.PropertyEditorUi.Integer",
            };
        }

        private IEnumerable<DataTypePropertyPresentationModel> ToUmbValues(MetaPropUIAttribute attr)
        {
            return attr.ReturnType switch
            {
                nameof(Int32) => CreateIntValues(attr),
                _ => attr.DataType == "RichText"
                    ? CreateRichTextValues(attr)
                    : Enumerable.Empty<DataTypePropertyPresentationModel>()
            };
        }

        private IEnumerable<DataTypePropertyPresentationModel> CreateIntValues(MetaPropUIAttribute metaProp)
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

        private IEnumerable<DataTypePropertyPresentationModel> CreateRichTextValues(MetaPropUIAttribute metaProp)
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
