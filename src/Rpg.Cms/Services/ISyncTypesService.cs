using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface ISyncTypesService
    {
        IEnumerable<IContentType> DocumentTypes();
        Task<IEnumerable<ContentTypeCreateModel>> DocumentTypeUpdatesAsync(Guid userKey);
        Task Sync(SyncSession session);
    }
}