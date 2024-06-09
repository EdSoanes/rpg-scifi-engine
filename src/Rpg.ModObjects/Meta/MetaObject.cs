using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObject
    {
        public string BaseType { get; set; }
        public string Archetype { get; set; }
        public string[] BaseTypes { get; set; }
        public string? TemplateType { get; set; }
        public MetaProperty[] Properties { get; set; }
        public MetaState[] States { get; set; }
        public MetaAction[] Actions { get; set; }

        public bool IsComponent { get; set; }

        public string[] ParentTo { get; internal set; } = Array.Empty<string>();

        public void Inherit(MetaProperty parentProp)
        {
            foreach (var prop in Properties)
            {
                if (string.IsNullOrEmpty(prop.UI.Group))
                    prop.UI.Group = parentProp.UI?.Group ?? string.Empty;

                if (string.IsNullOrEmpty(prop.UI.Tab))
                    prop.UI.Tab = parentProp.UI?.Tab ?? string.Empty;

                if (!prop.UI.Ignore)
                    prop.UI.Ignore = parentProp.UI?.Ignore ?? false;
            }
        }

        public override string ToString()
        {
            return $"({(IsComponent ? "Component" : "Entity")}) {Archetype}";
        }
    }
}
