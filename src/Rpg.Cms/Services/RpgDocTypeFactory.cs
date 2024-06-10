using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public class RpgDocTypeFactory : IRpgDocTypeFactory
    {
        private readonly IRpgPropertyTypeFactory _propertyTypeFactory;

        public RpgDocTypeFactory(IRpgPropertyTypeFactory propertyTypeFactory)
        {
            _propertyTypeFactory = propertyTypeFactory;
        }

        public ContentTypeCreateModel Create(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, DocTypeTemplate template)
        {
            var containers = CreateContainers(system, template);
            var properties = _propertyTypeFactory.Create(session, system, template.Properties, containers);

            var createDocType = new ContentTypeCreateModel
            {
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
                createDocType.AllowedContentTypes = template.AllowedDocTypeAliases.Select(x => new ContentTypeSort(x.Key, 0, x.Alias));

            return createDocType;
        }

        public ContentTypeCreateModel Create(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, MetaObject metaObject)
        {
            var docTypeTemplate = new DocTypeTemplate(system.Identifier, metaObject.Archetype, metaObject.ObjectType)
            {
                Name = session.GetDocTypeName(system, metaObject),
                Alias = session.GetDocTypeAlias(system, metaObject)
            };

            docTypeTemplate.AddProp<RichTextUIAttribute>("Description");
            docTypeTemplate.Properties.AddRange(_propertyTypeFactory.CreateTemplates(session, system, metaObject, string.Empty));

            return Create(session, system, parentFolder, docTypeTemplate);
        }

        public ContentTypeUpdateModel Update(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IContentType docType)
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

            return updateDocType;
        }

        public ContentTypeUpdateModel Update(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IContentType docType)
        {
            var docTypeTemplate = new DocTypeTemplate(system.Identifier, metaObject.Archetype, metaObject.ObjectType)
            {
                Name = session.GetDocTypeName(system, metaObject),
                Alias = session.GetDocTypeAlias(system, metaObject)
            };

            docTypeTemplate.AddProp<RichTextUIAttribute>("Description");
            docTypeTemplate.Properties.AddRange(_propertyTypeFactory.CreateTemplates(session, system, metaObject, string.Empty));

            return Update(session, system, docTypeTemplate, docType);
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
