﻿using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;

namespace Rpg.Cyborgs
{
    public class CyborgsSystem : IMetaSystem
    {
        public string Identifier { get => "Cyborgs"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Cyborgs & Sidearms"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Cyborgs & Sidearms tabletop rpg system"; }

        public MetaObj[] Objects { get; set; } = Array.Empty<MetaObj>();

        public MetaAction[] Actions { get; set; } = Array.Empty<MetaAction>();

        public ActionGroup[] ActionGroups { get; set; } = Array.Empty<ActionGroup>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();

        public MetaPropAttribute[] PropUIs { get; set; } = Array.Empty<MetaPropAttribute>();

        public MetaObj AsContentTemplate(MetaObj obj)
        {
            var res = new MetaObj()
                .AddProp("Summary", EditorType.RichText)
                .AddProp("Description", EditorType.RichText)
                .Merge(obj);

            return res;
        }
    }
}
