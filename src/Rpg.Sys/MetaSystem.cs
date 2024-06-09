using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class MetaSystem : IMetaSystem
    {
        public string Identifier { get => "RpgSys"; }

        public string Name { get => "Rpg System"; }

        public string Version { get => "0.1"; }

        public string Description { get => "A test rpg system"; }

        public MetaObject[] Objects { get; set; } = Array.Empty<MetaObject>();
        public MetaPropUIAttribute[] PropUIAttributes { get; set; }
    }
}
