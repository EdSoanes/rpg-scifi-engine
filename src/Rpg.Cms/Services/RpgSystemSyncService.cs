using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class RpgSystemSyncService : IRpgSystemSyncService
    {
        private readonly IDocTypeSynchronizer _docTypeSynchronizer;
        private readonly IDocTypeFolderSynchronizer _docTypeFolderSynchronizer;
        private readonly IDataTypeSynchronizer _dataTypeSynchronizer;
        private readonly IDataTypeFolderSynchronizer _dataTypeFolderSynchronizer;
        private readonly DocTypeModelFactory _docTypeModelFactory;

        public RpgSystemSyncService(
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

        public async Task Sync(Guid userKey)
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            if (system != null)
            {
                var session = new SyncSession(userKey, system);

                session.DataTypes = await _dataTypeSynchronizer.Sync(session);
                session.DocTypes = _docTypeSynchronizer.GetAllDocTypes(session);
                session.DocTypeFolders = _docTypeFolderSynchronizer.GetAllDocTypeFolders(session);

                session.RootDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, system.Identifier, -1);
                session.EntityDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, "Entities", session.RootDocTypeFolder!.Id);
                session.ComponentDocTypeFolder = await _docTypeFolderSynchronizer.Sync(session, "Components", session.RootDocTypeFolder!.Id);

                session.StateElementType = await _docTypeSynchronizer.Sync(session, new MetaObj("State").AddProp("Description", "RichText", "RichText"), session.ComponentDocTypeFolder);
                session.ActionElementType = await _docTypeSynchronizer.Sync(session, new MetaObj("Action").AddProp("Description", "RichText", "RichText"), session.ComponentDocTypeFolder);

                var metaObjActionLibrary = new MetaObjActionLibrary("ActionLibrary", "Action Library")
                    .AddAllowedArchetype(session.ActionElementType!.Alias);
                session.ActionLibraryDocType = await _docTypeSynchronizer.Sync(session, metaObjActionLibrary, session.RootDocTypeFolder);

                var metaObjEntityLibrary = new MetaObjEntityLibrary(system.Identifier);
                foreach (var metaObject in system.Objects)
                {
                    var docType = await _docTypeSynchronizer.Sync(session, metaObject, session.EntityDocTypeFolder);
                    metaObjEntityLibrary.AddAllowedArchetype(docType!.Alias);
                }

                session.EntityLibraryDocType = await _docTypeSynchronizer.Sync(session, metaObjEntityLibrary, session.RootDocTypeFolder);

                var systemDocType = new MetaObj(system.Identifier, system.Name)
                    .AddAllowedArchetype(session.ActionLibraryDocType!.Alias)
                    .AddAllowedArchetype(session.EntityLibraryDocType!.Alias);

                session.SystemDocType = await _docTypeSynchronizer.Sync(session, systemDocType, session.RootDocTypeFolder);
            }
        }
    }
}
