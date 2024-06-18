using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services
{
    public class RpgSystemSyncService : IRpgSystemSyncService
    {
        private readonly IDocTypeSynchronizer _docTypeSynchronizer;
        private readonly IDocTypeFolderSynchronizer _docTypeFolderSynchronizer;
        private readonly IDataTypeSynchronizer _dataTypeSynchronizer;
        private readonly IDataTypeFolderSynchronizer _dataTypeFolderSynchronizer;
        private readonly DocTypeModelFactory _docTypeModelFactory;

        private readonly IContentService _contentService;
        private readonly IContentEditingService _contentEditingService;

        public RpgSystemSyncService(
            IDocTypeSynchronizer docTypeSynchronizer,
            IDocTypeFolderSynchronizer docTypeFolderSynchronizer,
            IDataTypeSynchronizer dataTypeSynchronizer,
            IDataTypeFolderSynchronizer dataTypeFolderSynchronizer,
            DocTypeModelFactory docTypeModelFactory,
            IContentService contentService,
            IContentEditingService contentEditingService)
        {
            _docTypeSynchronizer = docTypeSynchronizer;
            _docTypeFolderSynchronizer = docTypeFolderSynchronizer;
            _dataTypeSynchronizer = dataTypeSynchronizer;
            _dataTypeFolderSynchronizer = dataTypeFolderSynchronizer;
            _docTypeModelFactory = docTypeModelFactory;
            _contentService = contentService;
            _contentEditingService = contentEditingService;
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
            var session = CreateSession(userKey);

            await SyncDataTypesAsync(session);
            await SyncDocTypeFoldersAsync(session);
            await SyncDocTypesAsync(session);

            await SyncContentAsync(session);
        }

        private SyncSession CreateSession(Guid userKey)
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            if (system != null)
            {
                var session = new SyncSession(userKey, system);
                return session;
            }

            throw new InvalidOperationException("Could not create session");
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
                .AddProp("Description", "RichText");

            session.StateElementType = await _docTypeSynchronizer.Sync(session, stateDocType, session.ComponentDocTypeFolder!);

            var actionDocType = new MetaObj("Action")
                .AddIcon("icon-command")
                .AddProp("Description", "RichText");

            session.ActionElementType = await _docTypeSynchronizer.Sync(session, actionDocType, session.ComponentDocTypeFolder!);

            var actionLibraryDocType = new MetaObj("Action Library")
                .AddIcon("icon-books")
                .AddProp("Description", "RichText")
                .AddAllowedArchetype(session.GetDocTypeAlias("Action Library"))
                .AddAllowedArchetype(session.ActionElementType!.Alias);

            session.ActionLibraryDocType = await _docTypeSynchronizer.Sync(session, actionLibraryDocType, session.RootDocTypeFolder!);

            var entityLibraryDocType = new MetaObj("Entity Library")
                .AddIcon("icon-books")
                .AddProp("Description", "RichText")
                .AddAllowedArchetype(session.GetDocTypeAlias("Entity Library"));

            foreach (var metaObject in session.System.Objects)
            {
                var docType = await _docTypeSynchronizer.Sync(session, metaObject, session.EntityDocTypeFolder!);
                entityLibraryDocType.AddAllowedArchetype(docType!.Alias);
            }

            session.EntityLibraryDocType = await _docTypeSynchronizer.Sync(session, entityLibraryDocType, session.RootDocTypeFolder!);

            var systemDocType = new MetaObj(session.System.Identifier)
                .AddIcon("icon-settings")
                .AddProp("Identifier", "Text")
                .AddProp("Version", "Text")
                .AddProp("Description", "RichText")
                .AllowAsRoot(true)
                .AddAllowedArchetype(session.GetDocTypeAlias(session.System.Identifier))
                .AddAllowedArchetype(session.ActionLibraryDocType!.Alias)
                .AddAllowedArchetype(session.EntityLibraryDocType!.Alias);

            session.SystemDocType = await _docTypeSynchronizer.Sync(session, systemDocType, session.RootDocTypeFolder!);
        }

        private async Task SyncContentAsync(SyncSession session)
        {
            var systemRoot = _contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == session.SystemDocType!.Alias);
            if (systemRoot == null)
            {
                var props = new Dictionary<string, object>
                {
                    { "Identifier", session.System.Identifier },
                    { "Version", session.System.Version },
                    { "Description", session.System.Description }
                };

                systemRoot = await CreateContentAsync(session.UserKey, session.System.Identifier, session.SystemDocType!.Key, Constants.System.RootKey, props);
            }

            var sysChildren = _contentService.GetPagedChildren(systemRoot!.Id, 0, 10000, out var totalRecords);
            var entityLibrary = sysChildren.FirstOrDefault(x => x.ContentType.Alias == session.EntityLibraryDocType!.Alias);
            if (entityLibrary == null)
            {
                entityLibrary = await CreateContentAsync(session.UserKey, "Entity Library", session.EntityLibraryDocType!.Key, systemRoot.Key);
            }

            var actionLibrary = sysChildren.FirstOrDefault(x => x.ContentType.Alias == session.ActionLibraryDocType!.Alias);
            if (actionLibrary == null)
            {
                entityLibrary = await CreateContentAsync(session.UserKey, "Action Library", session.ActionLibraryDocType!.Key, systemRoot.Key);
            }
        }

        private async Task<IContent> CreateContentAsync(Guid userKey, string name, Guid docTypeKey, Guid? parentKey, Dictionary<string, object>? props = null)
        {
            var model = new ContentCreateModel
            {
                ContentTypeKey = docTypeKey,
                InvariantName = name,
                ParentKey = parentKey
            };

            if (props != null)
            {
                var invariantProperties = new List<PropertyValueModel>();

                foreach (var prop in props)
                    invariantProperties.Add(new PropertyValueModel { Alias = prop.Key, Value = prop.Value });

                model.InvariantProperties = invariantProperties;
            }

            var attempt = await _contentEditingService.CreateAsync(model, userKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Failed to publish {name}");

            return attempt.Result.Content!;
        }
    }
}
