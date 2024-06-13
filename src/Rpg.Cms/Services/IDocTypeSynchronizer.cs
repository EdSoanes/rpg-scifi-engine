using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IDocTypeSynchronizer
    {
        ContentTypeCreateModel CreateModel(RpgSyncSession session, IMetaSystem system, IUmbracoEntity parentFolder, MetaObject metaObject);
        Task<IContentType?> Synchronize(RpgSyncSession session, IMetaSystem system, DocTypeTemplate template, IUmbracoEntity parentFolder);
        Task<IContentType?> Synchronize(RpgSyncSession session, IMetaSystem system, MetaObject metaObject, IUmbracoEntity parentFolder);
    }
}