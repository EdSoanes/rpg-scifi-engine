using Rpg.ModObjects.Meta;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IDataTypeSynchronizer
    {
        string GetName(IMetaSystem system, MetaPropUIAttribute propUIAttribute);
        CreateDataTypeRequestModel CreateModel(IMetaSystem system, IUmbracoEntity parentFolder, MetaPropUIAttribute propUIAttribute);
        Task<List<IDataType>> Synchronize(RpgSyncSession session, IMetaSystem system);
    }
}