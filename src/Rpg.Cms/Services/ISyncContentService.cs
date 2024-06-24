
namespace Rpg.Cms.Services
{
    public interface ISyncContentService
    {
        Task Sync(SyncSession session);
    }
}