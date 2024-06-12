using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class PropertyTypeTemplate
    {
        public Guid Key { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Alias { get; set; }
        public string? Tab { get; set; }
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
}
