using Rpg.Cms.Services.Templates;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Synchronizers
{
    public interface IDocTypeFolderSynchronizer
    {
        List<IUmbracoEntity> GetAllDocTypeFolders(SyncSession session);
        Task<IUmbracoEntity> Sync(SyncSession session, string folderName, int parentId);
    }
}