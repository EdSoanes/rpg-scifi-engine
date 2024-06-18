using Rpg.ModObjects.Meta;
using System.Security.Cryptography.Xml;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public class SyncSession
    {
        public IMetaSystem System { get; set; }
        public Guid UserKey { get; set; }

        public IUmbracoEntity? RootDataTypeFolder { get; set; }

        public IUmbracoEntity? RootDocTypeFolder { get; set; }
        public IUmbracoEntity? EntityDocTypeFolder { get; set; }
        public IUmbracoEntity? ComponentDocTypeFolder { get; set; }


        public IContentType? SystemDocType { get; set; }
        public IContentType? ActionLibraryDocType { get; set; }
        public IContentType? EntityLibraryDocType { get; set; }

        public IContentType? ObjectDocType { get; set; }
        public IContentType? EntityDocType { get; set; }
        public IContentType? ComponentDocType { get; set; }

        public IContentType? StateElementType { get; set; }
        public IContentType? ActionElementType { get; set; }

        public List<IContentType> DocTypes { get; set; } = new List<IContentType>();
        public List<IUmbracoEntity> DocTypeFolders { get; set; } = new List<IUmbracoEntity>();
        public List<IDataType> DataTypes { get; set; } = new List<IDataType>();


        public SyncSession(Guid userKey, IMetaSystem system)
        {
            System = system;
            UserKey = userKey;
        }

        public IDataType GetDataType(string propType)
        {
            var name = GetDataTypeName(propType);
            var res = DataTypes.FirstOrDefault(x => x.Name == name);
            if (res == null)
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

        public string GetDataTypeName(string propType)
            => $"{System.Identifier} {propType}";

        public string GetDocTypeName(MetaObj metaObject)
            => GetDocTypeName(metaObject.Archetype);

        public string GetDocTypeName(string archetype)
            => $"{System.Identifier} {archetype}";

        public string GetDocTypeAlias(MetaObj metaObject)
            => GetDocTypeAlias(metaObject.Archetype);

        public string GetDocTypeAlias(string archetype)
            => $"{System.Identifier}_{archetype}".Replace(" ", "");

        public string GetPropTypeAlias(MetaProp prop)
            => $"{System.Identifier}_{prop.Prop}";

        public string GetPropTypeTabName(string? tab)
            => string.IsNullOrEmpty(tab)
                ? System.Name
                : tab;

        public string GetPropTypeGroupName(string? group)
            => string.IsNullOrEmpty(group)
                ? System.Name
                : group;
    }




    public class DocTypeAliasTemplate
    {
        public Guid Key { get; set; }
        public string Alias { get; set; }

        public DocTypeAliasTemplate(Guid key, string alias)
        {
            Key = key;
            Alias = alias;
        }
    }
}
