using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class DocTypeTemplate
    {
        public Guid Key { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsElement { get; set; }
        public string Icon { get; set; }
        public bool AllowedAsRoot { get; set; }
        public List<PropertyTypeTemplate> Properties { get; private set; } = new List<PropertyTypeTemplate>();
        public List<string> AllowedDocTypeAliases { get; private set; } = new List<string>();

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

        public DocTypeTemplate AddAllowedAlias(string? alias)
        {
            if (!string.IsNullOrEmpty(alias) && !AllowedDocTypeAliases.Contains(alias))
                AllowedDocTypeAliases.Add(alias);

            return this;
        }

        public DocTypeTemplate AddAllowedSelf()
            => AddAllowedAlias(Alias);
    }
}
