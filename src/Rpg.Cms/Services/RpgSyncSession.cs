using Microsoft.AspNetCore.Components.Forms;
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

        public FolderTemplate RootFolderTemplate { get; set; }
        public FolderTemplate EntityFolderTemplate { get; set; }
        public FolderTemplate ComponentFolderTemplate { get; set; }

        public IUmbracoEntity? RootDataTypeFolder { get; set; }

        public IUmbracoEntity? RootDocTypeFolder { get; set; }
        public IUmbracoEntity? EntityDocTypeFolder { get; set; }
        public IUmbracoEntity? ComponentDocTypeFolder { get; set; }

        public DocTypeTemplate SystemTemplate { get; private set; }
        public DocTypeTemplate ObjectTemplate { get; private set; }
        public DocTypeTemplate EntityTemplate { get; private set; }
        public DocTypeTemplate ComponentTemplate { get; private set; }

        public DocTypeTemplate StateTemplate { get; private set; }
        public DocTypeTemplate ActionTemplate { get; private set; }

        public IContentType? SystemDocType { get; set; }
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

            RootFolderTemplate = new FolderTemplate(system.Identifier, system.Identifier);
            EntityFolderTemplate = new FolderTemplate(system.Identifier, "Entities");
            ComponentFolderTemplate = new FolderTemplate(system.Identifier, "Components");

            ObjectTemplate = new DocTypeTemplate(system.Identifier, "Object", "icon-fullscreen")
                .AddProp<RichTextUIAttribute>("Description");

            EntityTemplate = new DocTypeTemplate(system.Identifier, "Entity", "icon-stop")
                .AddProp<RichTextUIAttribute>("Description");

            ComponentTemplate = new DocTypeTemplate(system.Identifier, "Component", "icon-brick")
                .AddProp<RichTextUIAttribute>("Description");

            StateTemplate = new DocTypeTemplate(system.Identifier, "State", "icon-checkbox")
                .AddProp<RichTextUIAttribute>("Description");

            ActionTemplate = new DocTypeTemplate(system.Identifier, "Action", "icon-command")
                .AddProp<RichTextUIAttribute>("Description");

            SystemTemplate = new DocTypeTemplate(system.Identifier, system.Identifier, "icon-settings")
                .AddProp<TextUIAttribute>("Identifier")
                .AddProp<TextUIAttribute>("Version")
                .AddProp<RichTextUIAttribute>("Description");
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

    public class PropertyTypeTemplate
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string? Tab {  get; set; }
        public string? Group { get; set; }
        public MetaPropUIAttribute UI { get; set; }

        public PropertyTypeTemplate(string name, MetaPropUIAttribute propUI)
        {
            Name = name;
            Alias = name.ToLower();
            UI = propUI;
            Tab = propUI.Tab;
            Group = propUI.Group;
        }

        public PropertyTypeTemplate(string name, string alias, MetaPropUIAttribute propUI, string? parentTab, string? parentGroup)
        {
            Name = name;
            Alias = alias;
            UI = propUI;
            Tab = string.IsNullOrEmpty(propUI.Tab) ? parentTab : propUI.Tab;
            Group = string.IsNullOrEmpty(propUI.Group) ? parentTab : propUI.Group;
        }
    }

    public class FolderTemplate
    {
        public string Name { get; set; }
        public string Alias { get; set; }

        public FolderTemplate(string identifier, string name)
        {
            Name = name;
            Alias = identifier == name 
                ? identifier
                : $"{identifier}_{name}";
        }
    }

    public class DocTypeTemplate
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsElement { get; set; }
        public string Icon { get; set; }
        public bool AllowedAsRoot { get; set; }
        public List<PropertyTypeTemplate> Properties { get; private set; } = new List<PropertyTypeTemplate>();
        public List<DocTypeAliasTemplate> AllowedDocTypeAliases { get; private set; } = new List<DocTypeAliasTemplate>();

        public DocTypeTemplate(string identifier, string name, MetaObjectType objectType, string icon = "icon-checkbox-dotted", bool allowAtRoot = false)
        {
            Name = name;
            Alias = identifier == name ? identifier : $"{identifier}_{name}";
            IsElement = objectType == MetaObjectType.Component || objectType == MetaObjectType.ComponentTemplate;
            Icon = icon;
            AllowedAsRoot = allowAtRoot;
        }

        public DocTypeTemplate(string identifier, string name, string icon = "icon-checkbox-dotted", bool allowAtRoot = false)
        {
            Name = name;
            Alias = identifier == name ? identifier : $"{identifier}_{name}";
            IsElement = false;
            Icon = icon;
            AllowedAsRoot = allowAtRoot;
        }

        public DocTypeTemplate AddProp<T>(string name)
            where T : MetaPropUIAttribute
        {
            if (!Properties.Any(x => x.Name == name))
            {
                var propUI = Activator.CreateInstance<T>();
                Properties.Add(new PropertyTypeTemplate(name, propUI));
            }

            return this;
        }

        public DocTypeTemplate AddAllowedAlias(Guid? key, string? alias)
        {
            if (key != null && !string.IsNullOrEmpty(alias) && !AllowedDocTypeAliases.Any(x => x.Alias == alias))
                AllowedDocTypeAliases.Add(new DocTypeAliasTemplate(key.Value, alias));

            return this;
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
}
