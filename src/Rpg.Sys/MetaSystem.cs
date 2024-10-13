using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;

namespace Rpg.Sys
{
    public class MetaSystem : IMetaSystem
    {
        public string Identifier { get => "RpgSys"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Rpg System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "A test rpg system"; }

        public MetaObj[] Objects { get; set; } = Array.Empty<MetaObj>();

        public MetaAction[] Actions { get; set; } = Array.Empty<MetaAction>();

        public ActivityTemplate[] ActivityTemplates { get; set; } = Array.Empty<ActivityTemplate>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();
        
        public MetaPropAttr[] PropUIs { get; set; } = Array.Empty<MetaPropAttr>();

        public MetaObj AsContentTemplate(MetaObj obj)
        {
            return obj;
        }
    }
}
