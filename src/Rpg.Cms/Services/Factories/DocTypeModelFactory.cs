using Rpg.Cms.Extensions;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services.Factories
{
    public class DocTypeModelFactory
    {
        public ContentTypeCreateModel CreateModel(SyncSession session, MetaObj metaObject, string icon = "icon-checkbox-dotted")
        {
            var createDocType = new ContentTypeCreateModel
            {
                Key = Guid.NewGuid(),
                Name = session.System.GetDocumentTypeAlias(metaObject.Archetype),
                Alias = session.System.GetDocumentTypeAlias(metaObject.Archetype)
            };

            return SetModel(session, createDocType, metaObject, icon, null);
        }

        public ContentTypeUpdateModel UpdateModel(SyncSession session, MetaObj metaObject, IContentType docType, string icon = "icon-checkbox-dotted")
        {
            var updateDocType = new ContentTypeUpdateModel
            {
                Name = docType.Name!,
                Alias = docType.Alias,
            };
            
            updateDocType = SetModel(session, updateDocType, metaObject, icon ?? docType.Icon!, docType);

            return updateDocType;
        }

        private T SetModel<T>(SyncSession session, T docTypeModel, MetaObj metaObject, string icon, IContentType? docType)
            where T : ContentTypeModelBase
        {
            var containers = CreateContainers(session, metaObject);
            var properties = CreateProperties(session, metaObject.Props, containers, docType);

            docTypeModel.Icon = metaObject.Icon ?? icon;
            docTypeModel.Containers = containers;
            docTypeModel.Properties = properties;
            docTypeModel.AllowedAsRoot = metaObject.AllowedAsRoot;
            docTypeModel.IsElement = metaObject.IsElement;

            if (metaObject.AllowedChildArchetypes.Any())
            {
                var allowedTypes = new List<ContentTypeSort>();
                int i = 0;
                foreach (var archetype in metaObject.AllowedChildArchetypes)
                {
                    var childDocType = session.GetDocType(archetype, faultOnNotFound: false);
                    if (childDocType != null)
                        allowedTypes.Add(new ContentTypeSort(childDocType.Key, i++, childDocType.Alias));
                }

                docTypeModel.AllowedContentTypes = allowedTypes;
            }

            return docTypeModel;
        }

        private ContentTypePropertyContainerModel GetTab(IEnumerable<ContentTypePropertyContainerModel> containers, string tabName)
        {
            var tab = containers.FirstOrDefault(x => x.Name == tabName && x.Type == "Tab");
            if (tab == null)
                throw new InvalidOperationException($"Could not find tab {tabName}");

            return tab;
        }

        private ContentTypePropertyContainerModel GetTabGroup(IEnumerable<ContentTypePropertyContainerModel> containers, Guid tabKey, string groupName)
        {
            var tabGroups = containers.Where(x => x.ParentKey == tabKey && x.Type == "Group");
            var group = tabGroups.FirstOrDefault(x => x.Name == groupName);
            if (group == null)
                throw new InvalidOperationException($"Could not find group {groupName}");

            return group;
        }

        private List<ContentTypePropertyContainerModel> CreateContainers(SyncSession session, MetaObj metaObj, IContentType? docType = null)
        {
            var containers = new List<ContentTypePropertyContainerModel>();
            foreach (var tab in metaObj.Props.Select(x => x.Tab ?? string.Empty).Distinct())
            {
                var tabName = session.GetPropTypeTabName(tab);
                var existingTabContainer = docType?.PropertyGroups.FirstOrDefault(x => x.Name == tabName && x.Type == PropertyGroupType.Tab);

                var tabContainer = new ContentTypePropertyContainerModel
                {
                    Key = existingTabContainer?.Key ?? Guid.NewGuid(),
                    Name = tabName,
                    Type = "Tab"
                };

                containers.Add(tabContainer);

                var groups = metaObj.Props
                    .Where(x => (x.Tab ?? string.Empty) == tab)
                    .Select(x => x.Group ?? string.Empty)
                    .Distinct();

                foreach (var group in groups)
                {
                    var groupName = session.GetPropTypeGroupName(group);
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

        public ContentTypePropertyTypeModel[] CreateProperties(SyncSession session, IEnumerable<MetaProp> metaProps, IEnumerable<ContentTypePropertyContainerModel> containers, IContentType? docType = null)
        {
            var res = new List<ContentTypePropertyTypeModel>();
            var sortOrder = 0;
            foreach (var metaProp in metaProps.Where(x => !x.Ignore))
            {
                var propType = docType?.PropertyTypes.FirstOrDefault(x => x.Name == metaProp.Prop);
                var dataType = session.GetDataTypeByName(metaProp.DataTypeName, faultOnNotFound: false);
                var propModel = new ContentTypePropertyTypeModel
                {
                    Key = propType?.Key ?? Guid.NewGuid(),
                    Alias = metaProp.FullProp,
                    Description = metaProp.FullProp,
                    Name = metaProp.DisplayName,
                    DataTypeKey = dataType!.Key,
                    SortOrder = sortOrder++
                };

                if (propType?.Key != null)
                    propModel.Key = propType.Key;

                var group = GetPropertyContainerModel(session, containers, metaProp);
                propModel.ContainerKey = group.Key;

                res.Add(propModel);
            }

            return res.ToArray();
        }

        private ContentTypePropertyContainerModel GetPropertyContainerModel(SyncSession session, IEnumerable<ContentTypePropertyContainerModel> containers, MetaProp? metaProp)
        {
            var tabName = session.GetPropTypeTabName(metaProp.Tab);
            var groupName = session.GetPropTypeGroupName(metaProp.Group);

            var tab = GetTab(containers, tabName);
            var group = GetTabGroup(containers, tab.Key, groupName);

            return group;
        }
    }

    public static class MetaObjConverterExtensions
    {
        public static T AddAllowedDocTypeAlias<T>(this T docTypeModel, SyncSession session, string alias)
            where T : ContentTypeModelBase
        {
            var docType = session.GetDocType(alias, faultOnNotFound: false);
            if (docType != null && !docTypeModel.AllowedContentTypes.Any(x => x.Alias == docType.Alias))
            {
                var allowed = docTypeModel.AllowedContentTypes.ToList();
                allowed.Add(new ContentTypeSort(docType.Key, 0, docType.Alias));

                docTypeModel.AllowedContentTypes = allowed;
            }

            return docTypeModel;
        }
    }
}
