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
        public UpdateDataTypeRequestModel UpdateModel(SyncSession session, MetaPropUIAttribute attr)
        {
            var res = new UpdateDataTypeRequestModel();
            res = UpdateModel(session, attr);

            return res;
        }

        public CreateDataTypeRequestModel[] CreateModels(SyncSession session, IUmbracoEntity parentFolder)
        {
            var res = new List<CreateDataTypeRequestModel>();
            foreach (var attr in session.System.PropUIs)
            {
                //var dataTypeName = session.GetDataTypeName(attr.DataTypeName);
                //if (!res.Any(x => x.Name == dataTypeName))
                //{
                var model = CreateModel(session, attr, parentFolder);
                res.Add(model);
                //}
            }
            res.Add(CreateBooleanModel(session, parentFolder));

            foreach (var metaObj in session.System.Objects)
            {
                var actionDataType = CreateActionModel(session, metaObj, parentFolder);
                var stateDataType = CreateStateModel(session, metaObj, parentFolder);

                if (actionDataType != null)
                    res.Add(actionDataType);

                if (stateDataType != null)
                    res.Add(stateDataType);
            }

            return res.ToArray();
        }

        public CreateDataTypeRequestModel CreateModel(SyncSession session, MetaPropUIAttribute attr, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel();

            res.Id = Guid.NewGuid();
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.Name = session.GetDataTypeName(attr.DataTypeName);
            res = UpdateModel(res, session, attr);

            return res;
        }

        public CreateDataTypeRequestModel CreateBooleanModel(SyncSession session, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel
            {
                Id = Guid.NewGuid(),
                Parent = new ReferenceByIdModel(parentFolder.Key),
                Name = session.GetDataTypeName("Boolean"),  
                EditorAlias = "Umbraco.TrueFalse",
                EditorUiAlias = "Umb.PropertyEditorUi.Toggle"
            };

            return res;
        }

        public CreateDataTypeRequestModel? CreateActionModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
            => CreateCheckBoxListModel(session, metaObj, parentFolder, "Actions", metaObj.AllowedActions.Select(x => $"{session.System.Identifier}.{x}").ToArray());

        public CreateDataTypeRequestModel? CreateStateModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
            => CreateCheckBoxListModel(session, metaObj, parentFolder, "States", metaObj.AllowedStates.Select(x => $"{session.System.Identifier}.{x}").ToArray());

        public UpdateDataTypeRequestModel? UpdateActionModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
        {
            var res = new UpdateDataTypeRequestModel();
            return UpdateCheckBoxListModel(res, session, metaObj, "Actions", metaObj.AllowedActions.Select(x => $"{session.System.Identifier}.{x}").ToArray());
        }

        public UpdateDataTypeRequestModel? UpdateStateModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
        {
            var res = new UpdateDataTypeRequestModel();
            return UpdateCheckBoxListModel(res, session, metaObj, "States", metaObj.AllowedStates.Select(x => $"{session.System.Identifier}.{x}").ToArray());
        }

        private T UpdateModel<T>(T res, SyncSession session, MetaPropUIAttribute attr)
            where T : DataTypeModelBase
        {
            res.EditorAlias = ToUmbEditor(attr);
            res.EditorUiAlias = ToUmbUIEditor(attr);
            res.Values = ToUmbValues(attr);

            return res;
        }

        private CreateDataTypeRequestModel? CreateCheckBoxListModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder, string name, string[] items)
        {
            if (metaObj.AllowedActions.Any())
            {
                var res = new CreateDataTypeRequestModel();

                res.Id = Guid.NewGuid();
                res.Parent = new ReferenceByIdModel(parentFolder.Key);
                res = UpdateCheckBoxListModel(res, session, metaObj, name, items);

                return res;
            }

            return null;
        }

        private T UpdateCheckBoxListModel<T>(T res, SyncSession session, MetaObj metaObj, string name, string[] items)
            where T : DataTypeModelBase
        {
            res.Name = session.GetDataTypeName($"{metaObj.Archetype} {name}");
            res.EditorAlias = "Umbraco.CheckBoxList";
            res.EditorUiAlias = "Umb.PropertyEditorUi.CheckBoxList";

            res.Values = new List<DataTypePropertyPresentationModel>
                {
                    new DataTypePropertyPresentationModel { Alias = "items", Value = items }
                };

            return res;
        }

        private string ToUmbEditor(MetaPropUIAttribute attr)
        {
            return attr.ReturnType switch
            {
                nameof(Int32) => attr.DataType == "Select" 
                    ? "Umbraco.DropDown.Flexible" 
                    :"Umbraco.Integer",
                nameof(Dice) => "Umbraco.TextBox",
                nameof(Boolean) => "Umbraco.TrueFalse",
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
                nameof(Boolean) => "Umb.PropertyEditorUi.Toggle",
                "Select" =>"Umb.PropertyEditorUi.Dropdown",
                "RichText" => "Umb.PropertyEditorUi.TinyMCE",
                "Text" => "Umb.PropertyEditorUi.TextBox",
                _ => "Umb.PropertyEditorUi.Integer",
            };
        }

        private IEnumerable<DataTypePropertyPresentationModel> ToUmbValues(MetaPropUIAttribute attr)
        {
            return attr.ReturnType switch
            {
                nameof(Int32) => attr.DataType == "Select" 
                    ? CreateDropDownValues(attr) 
                    : CreateIntValues(attr),
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

        private IEnumerable<DataTypePropertyPresentationModel> CreateDropDownValues(MetaPropUIAttribute metaProp)
        {
            var selectUI = (metaProp as SelectUIAttribute)!;
            var vals = new List<DataTypePropertyPresentationModel>
            {
                new DataTypePropertyPresentationModel { Alias = "items", Value = selectUI.Values }
            };

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

        //private IEnumerable<DataTypePropertyPresentationModel> CreateBlockListValues(MetaPropUIAttribute metaProp)
        //{
        //    var blocks = (metaProp as SelectUIAttribute)?.Values.Select(x => new { contentElementKey = })
        //    var vals = new List<DataTypePropertyPresentationModel>
        //    {
        //        new DataTypePropertyPresentationModel 
        //        { 
        //            Alias = "useInlineEditingAsDefault", 
        //            Value = false 
        //        },
        //        new DataTypePropertyPresentationModel 
        //        { 
        //            Alias = "useLiveEditing", 
        //            Value = true 
        //        },
        //        new DataTypePropertyPresentationModel 
        //        { 
        //            Alias = "blocks", Value = new[]
        //            {
        //                new { contentElementTypeKey = "", label = "{{ArgName}}" }
        //            }
        //        }
        //    };

        //    return vals;
        //}
    }
}
