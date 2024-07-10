using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.Entities;
using Rpg.Cms.Extensions;

namespace Rpg.Cms.Services.Factories
{
    public class DataTypeModelFactory
    {
        public CreateDataTypeRequestModel[] CreateModels(SyncSession session, IUmbracoEntity parentFolder)
        {
            var res = new List<CreateDataTypeRequestModel>();
            foreach (var attr in session.System.PropUIs)
            {
                var model = CreateModel(session, attr, parentFolder);
                res.Add(model);
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

        public CreateDataTypeRequestModel CreateModel(SyncSession session, MetaPropAttribute attr, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel();

            res.Id = Guid.NewGuid();
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.Name = session.GetDataTypeName(attr.DataTypeName);
            res.EditorAlias = attr.UmbEditor();
            res.EditorUiAlias = attr.UmbUIEditor();
            res.Values = attr.UmbDataTypeValues();

            return res;
        }

        private CreateDataTypeRequestModel CreateBooleanModel(SyncSession session, IUmbracoEntity parentFolder)
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

        private CreateDataTypeRequestModel? CreateActionModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
        {
            var dataTypeName = session.GetDataTypeName($"{metaObj.Archetype} Actions");
            return CreateCheckBoxListModel(dataTypeName, parentFolder, metaObj.AllowedActions.Select(x => $"{session.System.Identifier}.{x}").ToArray());
        }

        private CreateDataTypeRequestModel? CreateStateModel(SyncSession session, MetaObj metaObj, IUmbracoEntity parentFolder)
        {
            var dataTypeName = session.GetDataTypeName($"{metaObj.Archetype} States");
            return CreateCheckBoxListModel(dataTypeName, parentFolder, metaObj.AllowedStates.Select(x => $"{session.System.Identifier}.{x}").ToArray());
        }

        private CreateDataTypeRequestModel? CreateCheckBoxListModel(string dataTypeName, IUmbracoEntity parentFolder, string[] items)
        {
            if (items.Any())
            {
                var res = new CreateDataTypeRequestModel();

                res.Id = Guid.NewGuid();
                res.Parent = new ReferenceByIdModel(parentFolder.Key);
                res.Name = dataTypeName;
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
    }
}
