using Rpg.ModObjects.Meta;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services.Synchronizers
{
    public interface IDataTypeSynchronizer
    {
        Task<IEnumerable<IDataType>> GetDataTypesAsync(SyncSession session);
        Task<List<IDataType>> Sync(SyncSession session);
    }
}