using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;

namespace Rpg.Cms.Services
{
    public interface IRpgPropertyTypeFactory
    {
        string GetAlias(string propName, string? aliasPrefix = null);
        string GetContainerName(IMetaSystem system, string? containerName);
        ContentTypePropertyTypeModel[] Create(RpgSyncSession session, IMetaSystem system, IEnumerable<PropertyTypeTemplate> templates, IEnumerable<ContentTypePropertyContainerModel> containers, IContentType? docType = null);
        PropertyTypeTemplate[] CreateTemplates(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, string? aliasPrefix = null, string? parentTab = null, string? parentGroup = null);
    }
}