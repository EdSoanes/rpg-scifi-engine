using Rpg.Experimental.System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    /// <summary>
    /// What is known about a game system and its document types and data types while they are being
    /// brought into line with the game system's meta data
    /// </summary>
    public class SyncSession
    {
        public RpgSystem System { get; set; }
        public Guid UserKey { get; set; }

        public IUmbracoEntity? RootDataTypeFolder { get; set; }

        public IUmbracoEntity? RootDocTypeFolder { get; set; }
        public IUmbracoEntity? EntityDocTypeFolder { get; set; }
        public IUmbracoEntity? ComponentDocTypeFolder { get; set; }

        public IContentType? SystemDocType { get; set; }
        public IContentType? StateLibraryDocType { get; set; }
        public IContentType? ActionLibraryDocType { get; set; }
        public IContentType? EntityLibraryDocType { get; set; }

        public IContentType? StateDocType { get; set; }
        public IContentType? ActionArgDocType { get; set; }
        public IContentType? ActionDocType { get; set; }

        public List<IContentType> DocTypes { get; set; } = new List<IContentType>();
        public List<IUmbracoEntity> DocTypeFolders { get; set; } = new List<IUmbracoEntity>();
        public List<IDataType> DataTypes { get; set; } = new List<IDataType>();

        public SyncSession(Guid userKey, RpgSystem system)
        {
            System = system;
            UserKey = userKey;
        }

        public IDataType? GetDataTypeByName(string dataTypeName, bool faultOnNotFound = true)
        {
            var name = GetDataTypeName(dataTypeName);
            var res = DataTypes.FirstOrDefault(x => x.Name == name);
            if (res == null && faultOnNotFound)
                throw new InvalidOperationException($"Missing data type {name}");

            return res;
        }

        public IContentType? GetDocType(string alias, bool faultOnNotFound = true)
        {
            var res = DocTypes.FirstOrDefault(x => x.Alias == alias);
            if (res == null && faultOnNotFound)
                throw new InvalidOperationException($"Missing doc type with alias {alias}");

            return res;
        }

        public string GetDataTypeName(string dataTypeName)
            => !dataTypeName.StartsWith(System.Identifier) ? $"{System.Identifier} {dataTypeName}" : dataTypeName;

        public string GetPropTypeTabName(string? tab)
            => string.IsNullOrEmpty(tab)
                ? System.Name
                : tab;

        public string GetPropTypeGroupName(string? group)
            => string.IsNullOrEmpty(group)
                ? System.Name
                : group;
    }
}
