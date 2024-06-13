using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class DocTypeSynchronizer : IDocTypeSynchronizer
    {
        private readonly IRpgPropertyTypeFactory _propertyTypeFactory;
        private readonly IContentTypeEditingService _contentTypeEditingService;

        public DocTypeSynchronizer(IRpgPropertyTypeFactory propertyTypeFactory, IContentTypeEditingService contentTypeEditingService)
        {
            _propertyTypeFactory = propertyTypeFactory;
            _contentTypeEditingService = contentTypeEditingService;
        }

        public ContentTypeCreateModel CreateModel(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, MetaObject metaObject)
        {
            var docTypeTemplate = new DocTypeTemplate(system.Identifier, metaObject.Archetype, metaObject.ObjectType)
            {
                Name = session.GetDocTypeName(system, metaObject),
                Alias = session.GetDocTypeAlias(system, metaObject)
            };

            docTypeTemplate.AddProp<RichTextUIAttribute>("Description");
            docTypeTemplate.Properties.AddRange(_propertyTypeFactory.CreateTemplates(session, system, metaObject, string.Empty));

            return CreateModel(session, system, parentFolder, docTypeTemplate);
        }

        public async Task<IContentType?> Synchronize(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IUmbracoEntity parentFolder)
        {
            ContentTypeUpdateModel? updateDocType = null;

            var docType = session.GetDocType(session.GetDocTypeAlias(system, metaObject), faultOnNotFound: false);
            if (docType != null)
                updateDocType = UpdateModel(session, system, metaObject, docType);
            else
            {
                var createDocType = CreateModel(session, system, parentFolder, metaObject);
                docType = await CreateAsync(session, createDocType);

                if (createDocType.AllowedContentTypes.Any(x => x.Alias == createDocType.Alias))
                    updateDocType = UpdateModel(session, system, metaObject, docType!);
            }

            if (updateDocType != null)
                docType = await UpdateAsync(session, docType, updateDocType);

            return docType;
        }

        public async Task<IContentType?> Synchronize(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IUmbracoEntity parentFolder)
        {
            ContentTypeUpdateModel? updateDocType = null;

            var docType = session.GetDocType(template.Alias, faultOnNotFound: false);
            if (docType != null)
                updateDocType = UpdateModel(session, system, template, docType);
            else
            {
                var createDocType = CreateModel(session, system, parentFolder, template);
                docType = await CreateAsync(session, createDocType);

                if (createDocType.AllowedContentTypes.Any(x => x.Alias == createDocType.Alias))
                    updateDocType = UpdateModel(session, system, template, docType!);
            }

            if (updateDocType != null)
                docType = await UpdateAsync(session, docType, updateDocType);

            return docType;
        }

        public ContentTypeCreateModel CreateModel(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, DocTypeTemplate template)
        {
            var containers = CreateContainers(system, template);
            var properties = _propertyTypeFactory.Create(session, system, template.Properties, containers);

            var createDocType = new ContentTypeCreateModel
            {
                Key = template.Key,
                Name = template.Name,
                Alias = template.Alias,
                ContainerKey = parentFolder.Key,
                IsElement = template.IsElement,
                Icon = template.Icon,
                AllowedAsRoot = template.AllowedAsRoot,
                Containers = containers,
                Properties = properties
            };

            if (template.AllowedDocTypeAliases.Any())
            {
                var docTypes = template.AllowedDocTypeAliases
                    .Where(x => x != createDocType.Alias)
                    .Select(x => session.GetDocType(x));

                createDocType.AllowedContentTypes = docTypes.Select(x => new ContentTypeSort(x!.Key, 0, x.Alias));
            }

            return createDocType;
        }

        private async Task<IContentType?> CreateAsync(RpgSyncSession session, ContentTypeCreateModel createDocType)
        {
            var attempt = await _contentTypeEditingService.CreateAsync(createDocType, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            var docType = attempt.Result!;

            session.DocTypes.Add(docType);
            return docType;
        }

        private ContentTypeUpdateModel UpdateModel(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IContentType docType)
        {
            var containers = CreateContainers(system, template, docType);
            var properties = _propertyTypeFactory.Create(session, system, template.Properties, containers, docType);

            var updateDocType = new ContentTypeUpdateModel
            {
                Name = template.Name,
                Alias = template.Alias,
                IsElement = template.IsElement,
                Icon = template.Icon,
                AllowedAsRoot = template.AllowedAsRoot,
                Containers = containers,
                Properties = properties
            };

            if (template.AllowedDocTypeAliases.Any())
            {
                var docTypes = template.AllowedDocTypeAliases.Select(x => session.GetDocType(x));
                updateDocType.AllowedContentTypes = docTypes.Select(x => new ContentTypeSort(x!.Key, 0, x.Alias));
            }

            return updateDocType;
        }

        private ContentTypeUpdateModel UpdateModel(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IContentType docType)
        {
            var docTypeTemplate = new DocTypeTemplate(system.Identifier, metaObject.Archetype, metaObject.ObjectType)
            {
                Name = session.GetDocTypeName(system, metaObject),
                Alias = session.GetDocTypeAlias(system, metaObject)
            };

            docTypeTemplate.AddProp<RichTextUIAttribute>("Description");
            docTypeTemplate.Properties.AddRange(_propertyTypeFactory.CreateTemplates(session, system, metaObject, string.Empty));

            return UpdateModel(session, system, docTypeTemplate, docType);
        }

        private async Task<IContentType?> UpdateAsync(RpgSyncSession session, IContentType? docType, ContentTypeUpdateModel updateDocType)
        {
            var attempt = await _contentTypeEditingService.UpdateAsync(docType, updateDocType, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            session.DocTypes.Remove(docType);

            docType = attempt.Result!;
            session.DocTypes.Add(docType);
            return docType;
        }

        private List<ContentTypePropertyContainerModel> CreateContainers(IMetaSystem system, DocTypeTemplate template, IContentType? docType = null)
        {
            var containers = new List<ContentTypePropertyContainerModel>();
            foreach (var tab in template.Properties.Select(x => x.Tab ?? string.Empty).Distinct())
            {
                var tabName = _propertyTypeFactory.GetContainerName(system, tab);
                var existingTabContainer = docType?.PropertyGroups.FirstOrDefault(x => x.Name == tabName && x.Type == PropertyGroupType.Tab);

                var tabContainer = new ContentTypePropertyContainerModel
                {
                    Key = existingTabContainer?.Key ?? Guid.NewGuid(),
                    Name = tabName,
                    Type = "Tab"
                };

                containers.Add(tabContainer);

                var groups = template.Properties
                    .Where(x => (x.Tab ?? string.Empty) == tab)
                    .Select(x => x.Group ?? string.Empty)
                    .Distinct();

                foreach (var group in groups)
                {
                    var groupName = _propertyTypeFactory.GetContainerName(system, group);
                    var existingGroupContainer = docType?.PropertyGroups.FirstOrDefault(x => x.Name == groupName && x.Type == PropertyGroupType.Group);

                    var groupContainer = new ContentTypePropertyContainerModel
                    {
                        Key = existingGroupContainer?.Key ?? Guid.NewGuid(),
                        ParentKey = tabContainer.Key,
                        Name = groupName,
                        Type = "Group"
                    };

                    containers.Add(groupContainer);
                }

            }

            return containers;
        }
    }
}
