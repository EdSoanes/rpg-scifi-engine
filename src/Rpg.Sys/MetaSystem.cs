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

        public ActionGroup[] ActionGroups { get; set; } = Array.Empty<ActionGroup>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();
        
        public MetaPropAttribute[] PropUIs { get; set; } = Array.Empty<MetaPropAttribute>();

        public MetaObj AsContentTemplate(MetaObj obj)
        {
            return obj;
        }
    }
}
