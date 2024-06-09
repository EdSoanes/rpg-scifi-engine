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

            ObjectTemplate = new DocTypeTemplate(system.Identifier, "Object", false, "icon-fullscreen")
                .AddProp<RichTextUIAttribute>("Description");

            EntityTemplate = new DocTypeTemplate(system.Identifier, "Entity", false, "icon-stop")
                .AddProp<RichTextUIAttribute>("Description");

            ComponentTemplate = new DocTypeTemplate(system.Identifier, "Component", false, "icon-brick")
                .AddProp<RichTextUIAttribute>("Description");

            StateTemplate = new DocTypeTemplate(system.Identifier, "State", true, "icon-checkbox")
                .AddProp<RichTextUIAttribute>("Description");

            ActionTemplate = new DocTypeTemplate(system.Identifier, "Action", true, "icon-command")
                .AddProp<RichTextUIAttribute>("Description");

            SystemTemplate = new DocTypeTemplate(system.Identifier, system.Identifier, false, "icon-settings")
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
    }

    public class PropertyTypeTemplate
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public MetaPropUIAttribute UI { get; set; }

        public PropertyTypeTemplate(string name, MetaPropUIAttribute propUI)
        {
            Name = name;
            Alias = name.ToLower();
            UI = propUI;
        }

        public PropertyTypeTemplate(string name, string alias, MetaPropUIAttribute propUI)
        {
            Name = name;
            Alias = alias;
            UI = propUI;
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
        public List<string> AllowedDocTypeAliases { get; private set; } = new List<string>();

        public DocTypeTemplate(string identifier, string name, bool isElement, string icon = "icon-checkbox-dotted", bool allowAtRoot = false)
        {
            Name = name;
            Alias = identifier == name ? identifier : $"{identifier}_{name}";
            IsElement = isElement;
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

        public DocTypeTemplate AddAllowedAlias(DocTypeTemplate docType)
        {
            if (!AllowedDocTypeAliases.Contains(docType.Alias))
                AllowedDocTypeAliases.Add(docType.Alias);

            return this;
        }
    }
}
