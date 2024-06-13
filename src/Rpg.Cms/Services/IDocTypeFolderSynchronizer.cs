using Rpg.Cms.Services.Templates;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IDocTypeFolderSynchronizer
    {
        Task<IUmbracoEntity> Synchronize(RpgSyncSession session, DocTypeFolderTemplate template, int parentId);
    }
}