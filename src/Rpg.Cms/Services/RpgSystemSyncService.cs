using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class RpgSystemSyncService : IRpgSystemSyncService
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeContainerService _dataTypeContainerService;
        private readonly IEntityService _entityService;
        private readonly IDataTypeSynchronizer _rpgDataTypeFactory;
        private readonly IDocTypeSynchronizer _docTypeSynchronizer;
        private readonly IDocTypeFolderSynchronizer _docTypeFolderSynchronizer;
        private readonly IDataTypeSynchronizer _dataTypeSynchronizer;

        public RpgSystemSyncService(
            IContentTypeService contentTypeService, 
            IDataTypeContainerService dataTypeContainerService,
            IEntityService entityService,
            IDataTypeSynchronizer rpgDataTypeFactory,
            IDocTypeSynchronizer docTypeSynchronizer,
            IDocTypeFolderSynchronizer docTypeFolderSynchronizer,
            IDataTypeSynchronizer dataTypeSynchronizer)
        {
            _contentTypeService = contentTypeService;
            _dataTypeContainerService = dataTypeContainerService;
            _entityService = entityService;
            _rpgDataTypeFactory = rpgDataTypeFactory;
            _docTypeSynchronizer = docTypeSynchronizer;
            _docTypeFolderSynchronizer = docTypeFolderSynchronizer;
            _dataTypeSynchronizer = dataTypeSynchronizer;
        }

        public IEnumerable<IContentType> DocumentTypes()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            return GetAllDocTypes(system);
        }

        public async Task<IEnumerable<ContentTypeCreateModel>> DocumentTypeUpdatesAsync(Guid userKey)
        {
            var meta = new MetaGraph();
            var system = meta.Build();
            if (system != null)
            {
                var session = new RpgSyncSession(userKey, system);

                session.RootDataTypeFolder = await EnsureDataTypeFolderAsync(session, system);
                session.DataTypes = await _dataTypeSynchronizer.Synchronize(session, system);

                session.DocTypes = GetAllDocTypes(system);
                session.DocTypeFolders = GetAllDocTypeFolders(system);

                session.RootDocTypeFolder = await _docTypeFolderSynchronizer.Synchronize(session, new RootFolderTemplate(system.Identifier), -1);
                var res = system.Objects
                    .Select(x => _docTypeSynchronizer.CreateModel(session, system, session.RootDocTypeFolder, x))
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
                var session = new RpgSyncSession(userKey, system);

                session.RootDataTypeFolder = await EnsureDataTypeFolderAsync(session, system);
                session.DataTypes = await _dataTypeSynchronizer.Synchronize(session, system);

                session.DocTypes = GetAllDocTypes(system);
                session.DocTypeFolders = GetAllDocTypeFolders(system);

                session.RootDocTypeFolder = await _docTypeFolderSynchronizer.Synchronize(session, new DocTypeFolderTemplate(system.Identifier, system.Identifier), -1);
                session.EntityDocTypeFolder = await _docTypeFolderSynchronizer.Synchronize(session, new DocTypeFolderTemplate(system.Identifier, "Entities"), session.RootDocTypeFolder!.Id);
                session.ComponentDocTypeFolder = await _docTypeFolderSynchronizer.Synchronize(session, new DocTypeFolderTemplate(system.Identifier, "Components"), session.RootDocTypeFolder!.Id);

                session.StateElementType = await _docTypeSynchronizer.Synchronize(session, system, new StateComponentTemplate(system.Identifier), session.ComponentDocTypeFolder);
                session.ActionElementType = await _docTypeSynchronizer.Synchronize(session, system, new ActionComponentTemplate(system.Identifier), session.ComponentDocTypeFolder);
                session.ActionLibraryDocType = await _docTypeSynchronizer.Synchronize(session, system, new ActionLibraryTemplate(system.Identifier), session.RootDocTypeFolder);

                var entityLibraryTemplate =new EntityLibraryTemplate(system.Identifier);
                foreach (var metaObject in system.Objects.Where(x => x.ObjectType == MetaObjectType.Entity))
                {
                    var docType = await _docTypeSynchronizer.Synchronize(session, system, metaObject, session.EntityDocTypeFolder);
                    entityLibraryTemplate.AddAllowedAlias(docType?.Alias);
                }
                session.EntityLibraryDocType = await _docTypeSynchronizer.Synchronize(session, system, entityLibraryTemplate, session.RootDocTypeFolder);

                var systemTemplate = new SystemTemplate(system.Identifier)
                    .AddAllowedAlias(session.ActionLibraryDocType?.Alias)
                    .AddAllowedAlias(session.EntityLibraryDocType?.Alias);

                session.SystemDocType = await _docTypeSynchronizer.Synchronize(session, system, systemTemplate, session.RootDocTypeFolder);
            }
        }

        private List<IContentType> GetAllDocTypes(IMetaSystem system)
        {
            return _contentTypeService.GetAll()
                .Where(x => x.Alias.StartsWith(system.Identifier))
                .ToList();
        }



        private List<IUmbracoEntity> GetAllDocTypeFolders(IMetaSystem system)
        {
            var folders = new List<IUmbracoEntity>();

            var rootFolder = _entityService.GetPagedChildren(
                Constants.System.RootKey,
                UmbracoObjectTypes.DocumentTypeContainer,
                0,
                int.MaxValue,
                out var totalItems)
                .FirstOrDefault(x => x.Name == system.Identifier);

            if (rootFolder != null)
            {
                folders.Add(rootFolder);
                var descendants = _entityService.GetDescendants(rootFolder.Id)
    .               Where(x => x.NodeObjectType == UmbracoObjectTypes.DocumentTypeContainer.GetGuid());

                folders.AddRange(descendants);
            }
            
            return folders;
        }

        private async Task<IUmbracoEntity> EnsureDataTypeFolderAsync(RpgSyncSession session, IMetaSystem system)
        {
            if (session.RootDataTypeFolder != null)
                return session.RootDataTypeFolder;

            var folder = GetRootDataTypeFolder(system);

            if (folder != null)
                return folder;

            var attempt = await _dataTypeContainerService.CreateAsync(null, system.Identifier, null, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Create datatype folder {system.Identifier} failed with {attempt.Status}");

            return attempt.Result!;
        }





        private IUmbracoEntity? GetRootDataTypeFolder(IMetaSystem system)
        {
            var folders = new List<IUmbracoEntity>();

            var rootFolder = _entityService.GetPagedChildren(
                Constants.System.RootKey,
                UmbracoObjectTypes.DataTypeContainer,
                0,
                int.MaxValue,
                out var totalItems)
                .FirstOrDefault(x => x.Name == system.Identifier);

            return rootFolder;
        }
    }
}
