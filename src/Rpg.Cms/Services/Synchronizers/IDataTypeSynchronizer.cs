using Umbraco.Cms.Core.Models;

namespace Rpg.Cms.Services.Synchronizers
{
    public interface IDataTypeSynchronizer
    {
        Task<IEnumerable<IDataType>> GetDataTypesAsync(SyncSession session);
        Task<List<IDataType>> Sync(SyncSession session);
        Task<List<IDataType>> ChildrenPickerSync(SyncSession session);
    }
}
