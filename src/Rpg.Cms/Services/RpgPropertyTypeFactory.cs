using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class RpgPropertyTypeFactory : IRpgPropertyTypeFactory
    {
        private readonly IDataTypeSynchronizer _typeFactory;

        public RpgPropertyTypeFactory(IDataTypeSynchronizer typeFactory)
        {
            _typeFactory = typeFactory;
        }

        public string GetAlias(string propName, string? aliasPrefix = null)
            => !string.IsNullOrEmpty(aliasPrefix)
                ? $"{aliasPrefix}_{propName}"
                : propName;

        public string GetContainerName(IMetaSystem system, string? containerName)
            => string.IsNullOrEmpty(containerName)
                ? system.Name
                : containerName;

        public ContentTypePropertyTypeModel[] Create(RpgSyncSession session, IMetaSystem system, IEnumerable<PropertyTypeTemplate> templates, IEnumerable<ContentTypePropertyContainerModel> containers, IContentType? docType = null)
        {
            var res = new List<ContentTypePropertyTypeModel>();

            foreach (var template in templates)
            {
                var tabName = GetContainerName(system, template.Tab);
                var groupName = GetContainerName(system, template.Group);

                var propModel = Create(session, system, template);
                var tab = containers.FirstOrDefault(x => x.Name == tabName && x.Type == "Tab");
                if (tab == null)
                    throw new InvalidOperationException($"Could not find tab {tabName} for prop {template.Name}");

                var tabGroups = containers.Where(x => x.ParentKey == tab.Key && x.Type == "Group");
                var group = tabGroups.FirstOrDefault(x => x.Name == groupName);
                if (group == null)
                    throw new InvalidOperationException($"Could not find group {groupName} for prop {template.Name}");

                var propGroup = docType?.PropertyGroups.FirstOrDefault(x => x.Name == group.Name && x.Type == PropertyGroupType.Group);
                var propType = propGroup?.PropertyTypes?.FirstOrDefault(x => x.Name == template.Name);
                if (propType != null)
                    propModel.Key = propType.Key;

                propModel.ContainerKey = group.Key;
                res.Add(propModel);
            }

            return res.ToArray();
        }

        private ContentTypePropertyTypeModel Create(RpgSyncSession session, IMetaSystem system, PropertyTypeTemplate template)
        {
            var dataType = session.GetDataType(_typeFactory.GetName(system, template.UI));
            var propTypeModel = new ContentTypePropertyTypeModel
            {
                Key = Guid.NewGuid(),
                Alias = template.Alias,
                Name = template.EditorName,
                DataTypeKey = dataType.Key,
            };

            return propTypeModel;
        }

        public PropertyTypeTemplate[] CreateTemplates(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, string? aliasPrefix = null, string? parentTab = null, string? parentGroup = null)
        {
            var propertyTypeTemplates = new List<PropertyTypeTemplate>();

            var properties = !string.IsNullOrEmpty(metaObject.TemplateType)
                ? system.Objects.First(x => x.Archetype == metaObject.TemplateType).Properties
                : metaObject.Properties;

            foreach (var prop in properties)
            {
                var alias = GetAlias(prop.Name, aliasPrefix);
                if (prop.ReturnObjectType == MetaObjectType.None && !prop.UI.Ignore)
                    propertyTypeTemplates.Add(new PropertyTypeTemplate(prop.Name, alias, prop.UI, parentTab, parentGroup));
                else
                {
                    var propDocType = system.Objects.First(x => x.TemplateType == prop.ReturnType);

                    var oldParentTab = parentTab;
                    var oldParentGroup = parentGroup;

                    parentTab = !string.IsNullOrEmpty(prop.UI.Tab) ? prop.UI.Tab : parentTab;
                    parentGroup = !string.IsNullOrEmpty(prop.UI.Group) ? prop.UI.Group : parentGroup;

                    propertyTypeTemplates.AddRange(CreateTemplates(session, system, propDocType, alias, parentTab, parentGroup));

                    parentTab = oldParentTab;
                    parentGroup = oldParentGroup;
                }
            }

            return propertyTypeTemplates.ToArray();
        }
    }
}
