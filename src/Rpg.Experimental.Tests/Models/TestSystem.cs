using Rpg.Experimental.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Tests.Models
{
    internal class TestSystem : IMetaSystem
    {
        public string Identifier { get => "Test"; }

        public string[]? Namespaces { get; set; }

        public string Name { get => "Test System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "Test system for unit testing"; }

        public MetaObject[] Objects { get; set; } = Array.Empty<MetaObject>();

        public MetaAction[] Actions { get; set; } = Array.Empty<MetaAction>();

        public MetaState[] States { get; set; } = Array.Empty<MetaState>();

        public Dictionary<string, object?>[] PropertyAttributes { get; set; } = [];
    }
}
