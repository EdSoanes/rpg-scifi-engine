using Umbraco.Cms.Core.Models;

namespace Rpg.Cms.Services
{
    public interface ISyncTypesService
    {
        IEnumerable<IContentType> DocumentTypes(SyncSession session);
        Task Sync(SyncSession session);
    }
}
