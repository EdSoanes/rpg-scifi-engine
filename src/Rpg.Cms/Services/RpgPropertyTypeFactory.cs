using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public class RpgPropertyTypeFactory : IRpgPropertyTypeFactory
    {
        private readonly IRpgDataTypeFactory _typeFactory;

        public RpgPropertyTypeFactory(IRpgDataTypeFactory typeFactory)
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
                var tabName = GetContainerName(system, template.UI.Tab);
                var groupName = GetContainerName(system, template.UI.Group);

                var propModel = Create(session, system, template);
                var tab = containers.First(x => x.Name == tabName && x.Type == "Tab");

                var tabGroups = containers.Where(x => x.ParentKey == tab.Key && x.Type == "Group");
                var group = tabGroups.First(x => x.Name == groupName);

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
                Name = template.Name,
                DataTypeKey = dataType.Key,
            };

            return propTypeModel;
        }

        public PropertyTypeTemplate[] CreateTemplates(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, string? aliasPrefix = null)
        {
            var propertyTypeTemplates = new List<PropertyTypeTemplate>();
            
            foreach (var prop in metaObject.Properties)
            {
                var alias = GetAlias(prop.Name, aliasPrefix);
                if (!prop.IsComponent && !prop.UI.Ignore)
                    propertyTypeTemplates.Add(new PropertyTypeTemplate(prop.Name, alias, prop.UI));
                else
                {
                    var propDocType = system.Objects.First(x => x.TemplateType == prop.ReturnType);
                    propertyTypeTemplates.AddRange(CreateTemplates(session, system, propDocType, alias));
                }
            }

            return propertyTypeTemplates.ToArray();
        }
    }
}
