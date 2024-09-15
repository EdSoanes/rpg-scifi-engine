using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.Entities;
using Rpg.Cms.Extensions;
using Umbraco.Cms.Core;

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
            res.Add(CreateContainerModel(session, parentFolder));

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

        public CreateDataTypeRequestModel CreateContainerModel(SyncSession session, IUmbracoEntity parentFolder)
        {
            var res = new List<CreateDataTypeRequestModel>();
            var aliases = session.System.Objects
                .Where(x => x.Archetypes.Contains("RpgEntity"))
                .Select(x => session.System.GetDocumentTypeAlias(x.Archetype));

            var items = session.DocTypes
                .Where(x => aliases.Contains(x.Alias))
                .Select(x => x.Key.ToString())
                .ToArray();

            var model = CreateModel(session, session.System.PropUIs.First(x => x.Editor! == EditorType.Container), parentFolder);
            var values = model.Values.Where(x => x.Alias != "filter").ToList();
            values.Add(new DataTypePropertyPresentationModel { Alias = "filter", Value = string.Join(',', items) });
            model.Values = values;

            return model;
        }

        public CreateDataTypeRequestModel CreateModel(SyncSession session, MetaPropAttr attr, IUmbracoEntity parentFolder)
        {
            var res = new CreateDataTypeRequestModel();

            res.Id = Guid.NewGuid();
            res.Parent = new ReferenceByIdModel(parentFolder.Key);
            res.Name = session.GetDataTypeName(attr.Value("DataTypeName", string.Empty));
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
                EditorAlias = Constants.PropertyEditors.Aliases.Boolean,
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
                res.EditorAlias = Constants.PropertyEditors.Aliases.CheckBoxList;
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
