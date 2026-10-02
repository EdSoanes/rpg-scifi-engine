using Rpg.Cms.Extensions;
using Rpg.Cms.Services.Synchronizers;
using Umbraco.Cms.Core.Models;

namespace Rpg.Cms.Services
{
    /// <summary>
    /// Brings the data types and document types of the content management system into line with a game
    /// system's meta data, so that an author can create the system's characters and items as content.
    /// </summary>
    public class SyncTypesService : ISyncTypesService
    {
        private readonly IDocTypeSynchronizer _docTypeSynchronizer;
        private readonly IDocTypeFolderSynchronizer _docTypeFolderSynchronizer;
        private readonly IDataTypeSynchronizer _dataTypeSynchronizer;
        private readonly IDataTypeFolderSynchronizer _dataTypeFolderSynchronizer;

        public SyncTypesService(
            IDocTypeSynchronizer docTypeSynchronizer,
            IDocTypeFolderSynchronizer docTypeFolderSynchronizer,
            IDataTypeSynchronizer dataTypeSynchronizer,
            IDataTypeFolderSynchronizer dataTypeFolderSynchronizer)
        {
            _docTypeSynchronizer = docTypeSynchronizer;
            _docTypeFolderSynchronizer = docTypeFolderSynchronizer;
            _dataTypeSynchronizer = dataTypeSynchronizer;
            _dataTypeFolderSynchronizer = dataTypeFolderSynchronizer;
        }

        public IEnumerable<IContentType> DocumentTypes(SyncSession session)
            => _docTypeSynchronizer.GetAllDocTypes(session);

        public async Task Sync(SyncSession session)
        {
            session.DocTypes = _docTypeSynchronizer.GetAllDocTypes(session);

            await SyncDataTypesAsync(session);
            await SyncDocTypeFoldersAsync(session);
            await SyncDocTypesAsync(session);

            //The picker for child objects is limited to the document types of the system's objects, which
            //only exist now
            session.DataTypes = await _dataTypeSynchronizer.ChildrenPickerSync(session);
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
            var system = session.System;

            var stateDocType = new DocTypeTemplate("State")
                .AddIcon("icon-rectangle-ellipsis")
                .AddProp("Description", RpgDataTypes.LongText);

            session.StateDocType = await _docTypeSynchronizer.Sync(session, stateDocType, session.ComponentDocTypeFolder!);

            var actionArgDocType = new DocTypeTemplate("Action Arg")
                .SetIsElement(true)
                .AddIcon("icon-rectangle-ellipsis")
                .AddProp("Arg Name", RpgDataTypes.Text)
                .AddProp("Description", RpgDataTypes.LongText)
                .AddProp("Type Name", RpgDataTypes.Text)
                .AddProp("Is Nullable", RpgDataTypes.Boolean);

            session.ActionArgDocType = await _docTypeSynchronizer.Sync(session, actionArgDocType, session.ComponentDocTypeFolder!);

            var actionDocType = new DocTypeTemplate("Action")
                .AddIcon("icon-command")
                .AddProp("Description", RpgDataTypes.LongText)
                .AddProp("Cost", RpgDataTypes.LongText)
                .AddProp("Perform", RpgDataTypes.LongText)
                .AddProp("Outcome", RpgDataTypes.LongText);

            session.ActionDocType = await _docTypeSynchronizer.Sync(session, actionDocType, session.ComponentDocTypeFolder!);

            var actionLibraryDocType = new DocTypeTemplate("Action Library")
                .AddIcon("icon-books")
                .AddProp("Description", RpgDataTypes.LongText)
                .AddAllowedChild(system.GetDocumentTypeAlias("Action Library"))
                .AddAllowedChild(session.ActionDocType!.Alias);

            session.ActionLibraryDocType = await _docTypeSynchronizer.Sync(session, actionLibraryDocType, session.RootDocTypeFolder!);

            var stateLibraryDocType = new DocTypeTemplate("State Library")
                .AddIcon("icon-books")
                .AddProp("Description", RpgDataTypes.LongText)
                .AddAllowedChild(system.GetDocumentTypeAlias("State Library"))
                .AddAllowedChild(session.StateDocType!.Alias);

            session.StateLibraryDocType = await _docTypeSynchronizer.Sync(session, stateLibraryDocType, session.RootDocTypeFolder!);

            var entityLibraryDocType = new DocTypeTemplate("Entity Library")
                .AddIcon("icon-books")
                .AddProp("Description", RpgDataTypes.LongText)
                .AddAllowedChild(system.GetDocumentTypeAlias("Entity Library"));

            //Only what an author can create gets a document type: the objects with a template
            foreach (var metaObject in system.AuthorableObjects())
            {
                var docType = await _docTypeSynchronizer.Sync(session, system.AsDocTypeTemplate(metaObject), session.EntityDocTypeFolder!);
                entityLibraryDocType.AddAllowedChild(docType!.Alias);
            }

            session.EntityLibraryDocType = await _docTypeSynchronizer.Sync(session, entityLibraryDocType, session.RootDocTypeFolder!);

            var systemDocType = new DocTypeTemplate(system.Identifier)
                .AddIcon("icon-settings")
                .AddProp("Identifier", RpgDataTypes.Text)
                .AddProp("Version", RpgDataTypes.Text)
                .AddProp("Description", RpgDataTypes.LongText)
                .AllowAsRoot(true)
                .AddAllowedChild(system.GetDocumentTypeAlias(system.Identifier))
                .AddAllowedChild(session.ActionLibraryDocType!.Alias)
                .AddAllowedChild(session.StateLibraryDocType!.Alias)
                .AddAllowedChild(session.EntityLibraryDocType!.Alias);

            session.SystemDocType = await _docTypeSynchronizer.Sync(session, systemDocType, session.RootDocTypeFolder!);
        }
    }
}
