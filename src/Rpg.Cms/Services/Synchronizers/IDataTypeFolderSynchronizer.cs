using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Synchronizers
{
    public interface IDataTypeFolderSynchronizer
    {
        Task<IUmbracoEntity> Sync(SyncSession session);
    }
}