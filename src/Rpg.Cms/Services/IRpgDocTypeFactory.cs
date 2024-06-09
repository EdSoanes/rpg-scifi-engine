using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IRpgDocTypeFactory
    {
        string GetAlias(IMetaSystem system, MetaObject metaObject);
        string GetName(IMetaSystem system, MetaObject metaObject);

        ContentTypeCreateModel Create(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, DocTypeTemplate template);
        ContentTypeCreateModel Create(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, MetaObject metaObject);
        ContentTypeUpdateModel Update(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IContentType docType);
        ContentTypeUpdateModel Update(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IContentType docType);
    }
}