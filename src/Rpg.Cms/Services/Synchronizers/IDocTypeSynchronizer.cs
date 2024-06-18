using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Synchronizers
{
    public interface IDocTypeSynchronizer
    {
        List<IContentType> GetAllDocTypes(SyncSession session);
        Task<IContentType?> Sync(SyncSession session, MetaObj metaObject, IUmbracoEntity parentFolder);    
    }
}