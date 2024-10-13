using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class SysTestSystem : IMetaSystem
    {
        public string Identifier { get => "TestSys"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Test System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Test System for Unit Tests"; }

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
