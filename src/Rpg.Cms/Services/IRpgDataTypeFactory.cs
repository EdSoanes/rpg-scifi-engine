using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public interface IRpgDataTypeFactory
    {
        string GetName(IMetaSystem system, MetaPropUIAttribute propUIAttribute);
        CreateDataTypeRequestModel Create(IMetaSystem system, IUmbracoEntity parentFolder, MetaPropUIAttribute propUIAttribute);
    }
}