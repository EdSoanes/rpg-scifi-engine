using Rpg.ModObjects.Meta;

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

        public MetaAction[] ActionTemplates { get; set; } = Array.Empty<MetaAction>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();

        public MetaPropAttr[] PropUIs { get; set; } = Array.Empty<MetaPropAttr>();

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
