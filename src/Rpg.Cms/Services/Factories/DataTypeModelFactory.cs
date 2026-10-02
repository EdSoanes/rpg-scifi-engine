using Rpg.Cms.Extensions;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Factories
{
    /// <summary>
    /// The data types a game system needs: one per kind of value an author can enter. Limits such as a
    /// minimum or maximum are deliberately not turned into validation, because the rules engine reports
    /// limits and never enforces them.
    /// </summary>
    public class DataTypeModelFactory
    {
        public CreateDataTypeRequestModel[] CreateModels(SyncSession session, IUmbracoEntity parentFolder)
            =>
            [
                CreateModel(session, parentFolder, RpgDataTypes.Integer, Constants.PropertyEditors.Aliases.Integer, "Umb.PropertyEditorUi.Integer"),
                CreateModel(session, parentFolder, RpgDataTypes.Dice, Constants.PropertyEditors.Aliases.TextBox, "Umb.PropertyEditorUi.TextBox"),
                CreateModel(session, parentFolder, RpgDataTypes.Text, Constants.PropertyEditors.Aliases.TextBox, "Umb.PropertyEditorUi.TextBox"),
                CreateModel(session, parentFolder, RpgDataTypes.LongText, Constants.PropertyEditors.Aliases.TextArea, "Umb.PropertyEditorUi.TextArea"),
                CreateModel(session, parentFolder, RpgDataTypes.Boolean, Constants.PropertyEditors.Aliases.Boolean, "Umb.PropertyEditorUi.Toggle"),
                CreateChildrenModel(session, parentFolder)
            ];

        /// <summary>
        /// The picker for the objects a parent holds. It only offers content of the document types of the
        /// system's own objects, so it is created again once those document types exist.
        /// </summary>
        public CreateDataTypeRequestModel CreateChildrenModel(SyncSession session, IUmbracoEntity parentFolder)
        {
            var aliases = session.System.AuthorableObjects()
                .Select(x => session.System.GetDocumentTypeAlias(x.Archetype))
                .ToArray();

            var docTypeKeys = session.DocTypes
                .Where(x => aliases.Contains(x.Alias))
                .Select(x => x.Key.ToString())
                .ToArray();

            var model = CreateModel(session, parentFolder, RpgDataTypes.Children, Constants.PropertyEditors.Aliases.MultiNodeTreePicker, "Umb.PropertyEditorUi.ContentPicker");
            model.Values =
            [
                new DataTypePropertyPresentationModel { Alias = "minNumber", Value = 0 },
                new DataTypePropertyPresentationModel { Alias = "maxNumber", Value = 0 },
                new DataTypePropertyPresentationModel { Alias = "filter", Value = string.Join(',', docTypeKeys) }
            ];

            return model;
        }

        private static CreateDataTypeRequestModel CreateModel(SyncSession session, IUmbracoEntity parentFolder, string name, string editorAlias, string editorUiAlias)
            => new CreateDataTypeRequestModel
            {
                Id = Guid.NewGuid(),
                Parent = new ReferenceByIdModel(parentFolder.Key),
                Name = session.GetDataTypeName(name),
                EditorAlias = editorAlias,
                EditorUiAlias = editorUiAlias,
                Values = []
            };
    }
}
