using Rpg.ModObjects.Meta;
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

        public string Name { get => "Test System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Test System for Unit Tests"; }

        public MetaObj[] Objects { get; set; } = Array.Empty<MetaObj>();

        public ModObjects.Actions.Action[] Actions { get; set; } = Array.Empty<ModObjects.Actions.Action>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();

        public MetaPropUIAttribute[] PropUIs { get; set; } = Array.Empty<MetaPropUIAttribute>();
    }
}
