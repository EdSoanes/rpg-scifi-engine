using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IRpgSystemSyncService
    {
        IEnumerable<IContentType> DocumentTypes();
        Task<IEnumerable<ContentTypeCreateModel>> DocumentTypeUpdatesAsync(Guid userKey);
        Task Sync(Guid userKey);
    }
}