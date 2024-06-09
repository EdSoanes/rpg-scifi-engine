using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IRpgSystemSyncService
    {
        Task Sync(Guid userKey);
    }
}