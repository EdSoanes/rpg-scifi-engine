using Microsoft.AspNetCore.Components.Forms;
using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Rpg.Cms.Services
{
    public class RpgSyncSession
    {
        public IMetaSystem System { get; set; }
        public Guid UserKey { get; set; }

        public DocTypeFolderTemplate RootFolderTemplate { get; set; }
        public DocTypeFolderTemplate EntityFolderTemplate { get; set; }
        public DocTypeFolderTemplate ComponentFolderTemplate { get; set; }

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


        public RpgSyncSession(Guid userKey, IMetaSystem system)
        {
            System = system;
            UserKey = userKey;

            RootFolderTemplate = new DocTypeFolderTemplate(system.Identifier, system.Identifier);
            EntityFolderTemplate = new DocTypeFolderTemplate(system.Identifier, "Entities");
            ComponentFolderTemplate = new DocTypeFolderTemplate(system.Identifier, "Components");
        }

        public IDataType GetDataType(string name)
        {
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

        public string GetDocTypeName(IMetaSystem system, MetaObject metaObject)
            => GetDocTypeName(system, metaObject.Archetype);

        public string GetDocTypeName(IMetaSystem system, string archetype)
            => $"{system.Identifier} {archetype}";

        public string GetDocTypeAlias(IMetaSystem system, MetaObject metaObject)
            => $"{system.Identifier}_{metaObject.Archetype}";
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
