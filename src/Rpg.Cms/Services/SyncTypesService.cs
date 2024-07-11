using Rpg.Cms.Extensions;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.ModObjects.Meta;
using System.Security.Cryptography.Xml;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class SyncTypesService : ISyncTypesService
    {
        private readonly IDocTypeSynchronizer _docTypeSynchronizer;
        private readonly IDocTypeFolderSynchronizer _docTypeFolderSynchronizer;
        private readonly IDataTypeSynchronizer _dataTypeSynchronizer;
        private readonly IDataTypeFolderSynchronizer _dataTypeFolderSynchronizer;
        private readonly DocTypeModelFactory _docTypeModelFactory;

        public SyncTypesService(
            IDocTypeSynchronizer docTypeSynchronizer,
            IDocTypeFolderSynchronizer docTypeFolderSynchronizer,
            IDataTypeSynchronizer dataTypeSynchronizer,
            IDataTypeFolderSynchronizer dataTypeFolderSynchronizer,
            DocTypeModelFactory docTypeModelFactory)
        {
            _docTypeSynchronizer = docTypeSynchronizer;
            _docTypeFolderSynchronizer = docTypeFolderSynchronizer;
            _dataTypeSynchronizer = dataTypeSynchronizer;
            _dataTypeFolderSynchronizer = dataTypeFolderSynchronizer;
            _docTypeModelFactory = docTypeModelFactory;
        }

        public IEnumerable<IContentType> DocumentTypes()
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            var session = new SyncSession(Guid.Empty, system);

            return _docTypeSynchronizer.GetAllDocTypes(session);
        }

        public async Task<IEnumerable<ContentTypeCreateModel>> DocumentTypeUpdatesAsync(Guid userKey)
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            if (system != null)
            {
                var session = new SyncSession(userKey, system);
                session.DataTypes = await _dataTypeSynchronizer.Sync(session);

                var res = system.Objects
                    .Select(x => _docTypeModelFactory.CreateModel(session, x))
                    .ToList();

                return res;
            }

            return Enumerable.Empty<ContentTypeCreateModel>();
        }

        public async Task Sync(SyncSession session)
        {
            session.DocTypes = _docTypeSynchronizer.GetAllDocTypes(session);

            await SyncDataTypesAsync(session);
            await SyncDocTypeFoldersAsync(session);
            await SyncDocTypesAsync(session);

            session.DataTypes = await _dataTypeSynchronizer.ContainerPickerSync(session);
        }

        private async Task SyncDataTypesAsync(SyncSession session)
        {
            session.RootDataTypeFolder = await _dataTypeFolderSynchronizer.Sync(session);
            session.DataTypes = await _dataTypeSynchronizer.Sync(session);
        }

        private async Task SyncDocTypeFoldersAsync(SyncSession session)
        {
            session.DocTypeFolders = _docTypeFolderSynchronizer.GetAllDocTypeFolders(session);
            session.DocTypes = _docTypeSynchronizer.GetAllDocTypes(session);

            session.RootDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, session.System.Identifier, -1);
            session.EntityDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, "Entities", session.RootDocTypeFolder!.Id);
            session.ComponentDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, "Components", session.RootDocTypeFolder!.Id);
        }

        private async Task SyncDocTypesAsync(SyncSession session)
        {
            var stateDocType = new MetaObj("State")
                .AddIcon("icon-rectangle-ellipsis")
                .AddProp("Description", EditorType.RichText);

            session.StateDocType = await _docTypeSynchronizer.Sync(session, stateDocType, session.ComponentDocTypeFolder!);

            var actionArgDocType = new MetaObj("Action Arg")
                .SetIsElement(true)
                .AddIcon("icon-rectangle-ellipsis")
                .AddProp("Arg Name", EditorType.Text)
                .AddProp("Description", EditorType.RichText)
                .AddProp("Type Name", EditorType.Text)
                .AddProp("Qualified Type Name", EditorType.Text)
                .AddProp("Is Nullable", EditorType.Boolean);

            session.ActionArgDocType = await _docTypeSynchronizer.Sync(session, actionArgDocType, session.ComponentDocTypeFolder!);

            var actionDocType = new MetaObj("Action")
                .AddIcon("icon-command")
                .AddProp("Description", EditorType.RichText)
                .AddProp("Action.Cost", EditorType.RichText)
                .AddProp("Action.Act", EditorType.RichText)
                .AddProp("Action.Outcome", EditorType.RichText);

            session.ActionDocType = await _docTypeSynchronizer.Sync(session, actionDocType, session.ComponentDocTypeFolder!);

            var actionLibraryDocType = new MetaObj("Action Library")
                .AddIcon("icon-books")
            .AddProp("Description", EditorType.RichText)
                .AddAllowedArchetype(session.System.GetDocumentTypeAlias("Action Library"))
                .AddAllowedArchetype(session.ActionDocType!.Alias);

            session.ActionLibraryDocType = await _docTypeSynchronizer.Sync(session, actionLibraryDocType, session.RootDocTypeFolder!);

            var stateLibraryDocType = new MetaObj("State Library")
                .AddIcon("icon-books")
                .AddProp("Description", EditorType.RichText)
                .AddAllowedArchetype(session.System.GetDocumentTypeAlias("State Library"))
                .AddAllowedArchetype(session.StateDocType!.Alias);

            session.StateLibraryDocType = await _docTypeSynchronizer.Sync(session, stateLibraryDocType, session.RootDocTypeFolder!);

            var entityLibraryDocType = new MetaObj("Entity Library")
                .AddIcon("icon-books")
                .AddProp("Description", EditorType.RichText)
                .AddAllowedArchetype(session.System.GetDocumentTypeAlias("Entity Library"));

            foreach (var metaObject in session.System.Objects)
            {
                var entity = session.System.AsContentTemplate(metaObject);
                var docType = await _docTypeSynchronizer.Sync(session, entity, session.EntityDocTypeFolder!);
                entityLibraryDocType.AddAllowedArchetype(docType!.Alias);
            }

            session.EntityLibraryDocType = await _docTypeSynchronizer.Sync(session, entityLibraryDocType, session.RootDocTypeFolder!);

            var systemDocType = new MetaObj(session.System.Identifier)
                .AddIcon("icon-settings")
                .AddProp("Identifier", EditorType.Text)
                .AddProp("Version", EditorType.Text)
                .AddProp("Description", EditorType.RichText)
                .AllowAsRoot(true)
                .AddAllowedArchetype(session.System.GetDocumentTypeAlias(session.System.Identifier))
                .AddAllowedArchetype(session.ActionLibraryDocType!.Alias)
                .AddAllowedArchetype(session.StateLibraryDocType!.Alias)
                .AddAllowedArchetype(session.EntityLibraryDocType!.Alias);

            session.SystemDocType = await _docTypeSynchronizer.Sync(session, systemDocType, session.RootDocTypeFolder!);
        }
    }
}
