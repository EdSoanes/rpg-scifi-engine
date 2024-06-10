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
        private readonly IContentTypeEditingService _contentTypeEditingService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
        private readonly IDataTypeContainerService _dataTypeContainerService;
        private readonly IContentTypeContainerService _contentTypeContainerService;
        private readonly IEntityService _entityService;
        private readonly IRpgDataTypeFactory _rpgDataTypeFactory;
        private readonly IRpgDocTypeFactory _rpgDocTypeFactory;

        public RpgSystemSyncService(
            IContentTypeService contentTypeService, 
            IContentTypeEditingService contentTypeEditingService, 
            IDataTypeService dataTypeService,
            IDataTypeContainerService dataTypeContainerService,
            IDataTypePresentationFactory dataTypePresentationFactory,
            IContentTypeContainerService contentTypeContainerService, 
            IEntityService entityService,
            IRpgDataTypeFactory rpgDataTypeFactory,
            IRpgDocTypeFactory rpgDocTypeFactory)
        {
            _contentTypeService = contentTypeService;
            _contentTypeEditingService = contentTypeEditingService;
            _dataTypeService = dataTypeService;
            _dataTypeContainerService = dataTypeContainerService;
            _dataTypePresentationFactory = dataTypePresentationFactory;
            _contentTypeContainerService = contentTypeContainerService;
            _entityService = entityService;
            _rpgDataTypeFactory = rpgDataTypeFactory;
            _rpgDocTypeFactory = rpgDocTypeFactory;
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
                session.DataTypes = await EnsureDataTypesAsync(session, system);

                session.DocTypes = GetAllDocTypes(system);
                session.DocTypeFolders = GetAllDocTypeFolders(system);

                session.RootDocTypeFolder = await EnsureDocTypeFolderAsync(session, session.RootFolderTemplate, -1);
                var res = system.Objects
                    .Select(x => _rpgDocTypeFactory.Create(session, system, session.RootDocTypeFolder, x))
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
                session.DataTypes = await EnsureDataTypesAsync(session, system);

                session.DocTypes = GetAllDocTypes(system);
                session.DocTypeFolders = GetAllDocTypeFolders(system);

                session.RootDocTypeFolder = await EnsureDocTypeFolderAsync(session, session.RootFolderTemplate, -1);
                session.EntityDocTypeFolder = await EnsureDocTypeFolderAsync(session, session.EntityFolderTemplate, session.RootDocTypeFolder!.Id);
                session.ComponentDocTypeFolder = await EnsureDocTypeFolderAsync(session, session.ComponentFolderTemplate, session.RootDocTypeFolder!.Id);

                //session.ObjectDocType = await EnsureDocTypeAsync(session, system, session.ObjectTemplate, session.RootDocTypeFolder);
                //session.EntityDocType = await EnsureDocTypeAsync(session, system, session.EntityTemplate, session.RootDocTypeFolder); 
                //session.ComponentDocType = await EnsureDocTypeAsync(session, system, session.ComponentTemplate, session.RootDocTypeFolder);

                session.StateElementType = await EnsureDocTypeAsync(session, system, session.StateTemplate, session.ComponentDocTypeFolder);
                session.ActionElementType = await EnsureDocTypeAsync(session, system, session.ActionTemplate, session.ComponentDocTypeFolder);

                session.ActionLibraryTemplate.AddAllowedAlias(session.ActionElementType?.Key, session.ActionElementType?.Alias);
                session.ActionLibraryDocType = await EnsureDocTypeAsync(session, system, session.ActionLibraryTemplate, session.RootDocTypeFolder);

                foreach (var metaObject in system.Objects.Where(x => x.ObjectType == MetaObjectType.Entity))
                {
                    var docType = await EnsureDocTypeAsync(session, system, metaObject, session.EntityDocTypeFolder);
                    session.EntityLibraryTemplate.AddAllowedAlias(docType?.Key, docType?.Alias);
                }
                session.EntityLibraryDocType = await EnsureDocTypeAsync(session, system, session.EntityLibraryTemplate, session.RootDocTypeFolder);

                session.SystemTemplate
                    .AddAllowedAlias(session.ActionLibraryDocType?.Key, session.ActionLibraryDocType?.Alias)
                    .AddAllowedAlias(session.EntityLibraryDocType?.Key, session.EntityLibraryDocType?.Alias);
                session.SystemDocType = await EnsureDocTypeAsync(session, system, session.SystemTemplate, session.RootDocTypeFolder);
            }
        }

        private List<IContentType> GetAllDocTypes(IMetaSystem system)
        {
            return _contentTypeService.GetAll()
                .Where(x => x.Alias.StartsWith(system.Identifier))
                .ToList();
        }

        private async Task<List<IDataType>> EnsureDataTypesAsync(RpgSyncSession session, IMetaSystem system)
        {
            var res = new List<IDataType>();

            var types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.PlainInteger);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Integer);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.TextBox);
            res.AddRange(types);

            types = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText);
            res.AddRange(types);

            res = res
                .Where(x => x.Name?.StartsWith(system.Identifier) ?? false)
                .ToList();

            foreach (var propUIAttribute in system.PropUIAttributes) 
            {
                var name = _rpgDataTypeFactory.GetName(system, propUIAttribute);
                if (!res.Any(x => x.Name == name))
                {
                    var dataTypeModel = _rpgDataTypeFactory.Create(system, session.RootDataTypeFolder!, propUIAttribute);
                    var presAttempt = await _dataTypePresentationFactory.CreateAsync(dataTypeModel);
                    if (!presAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype presentation for {name}");

                    var dataTypeAttempt = await _dataTypeService.CreateAsync(presAttempt.Result, session.UserKey);
                    if (!dataTypeAttempt.Success)
                        throw new InvalidOperationException($"Failed to create datatype for {name}");

                    res.Add(dataTypeAttempt.Result);
                }
            }

            return res;
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

        private async Task<IUmbracoEntity> EnsureDocTypeFolderAsync(RpgSyncSession session, FolderTemplate template, int parentId)
        {
            var parentFolder = session.DocTypeFolders.FirstOrDefault(x => x.Id == parentId);
            var folder = session.DocTypeFolders.FirstOrDefault(x => x.ParentId == (parentFolder?.Id ?? -1) && x.Name == template.Name);
            if (folder != null)
                return folder;

            var attempt = await _contentTypeContainerService.CreateAsync(null, template.Name, parentFolder?.Key, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            var entityContainer = attempt.Result!;
            session.DocTypeFolders.Add(entityContainer);

            return entityContainer;
        }

        private async Task<IContentType?> EnsureDocTypeAsync(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IUmbracoEntity parentFolder)
        {
            var docType = session.GetDocType(template.Alias, faultOnNotFound: false);
            if (docType == null)
            {
                var createDocType = _rpgDocTypeFactory.Create(session, system, parentFolder, template);

                var attempt = await _contentTypeEditingService.CreateAsync(createDocType, session.UserKey);
                if (!attempt.Success)
                    throw new InvalidOperationException(attempt.Status.ToString());

                docType = attempt.Result!;

                session.DocTypes.Add(docType);
            }
            else
            {
                var updateDocType = _rpgDocTypeFactory.Update(session, system, template, docType);

                var attempt = await _contentTypeEditingService.UpdateAsync(docType, updateDocType, session.UserKey);
                if (!attempt.Success)
                    throw new InvalidOperationException(attempt.Status.ToString());

                session.DocTypes.Remove(docType);

                docType = attempt.Result!;
                session.DocTypes.Add(docType);
            }

            return docType;
        }

        private async Task<IContentType?> EnsureDocTypeAsync(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IUmbracoEntity parentFolder)
        {
            var docType = session.GetDocType(session.GetDocTypeAlias(system, metaObject), faultOnNotFound: false);
            if (docType == null)
            {
                var createDocType = _rpgDocTypeFactory.Create(session, system, parentFolder, metaObject);

                var attempt = await _contentTypeEditingService.CreateAsync(createDocType, session.UserKey);
                if (!attempt.Success)
                    throw new InvalidOperationException(attempt.Status.ToString());

                docType = attempt.Result!;
                session.DocTypes.Add(docType);
            }
            else
            {
                var updateDocType = _rpgDocTypeFactory.Update(session, system, metaObject, docType);

                var attempt = await _contentTypeEditingService.UpdateAsync(docType, updateDocType, session.UserKey);
                if (!attempt.Success)
                    throw new InvalidOperationException(attempt.Status.ToString());

                session.DocTypes.Remove(docType);

                docType = attempt.Result!;
                session.DocTypes.Add(docType);
            }

            return docType;
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
