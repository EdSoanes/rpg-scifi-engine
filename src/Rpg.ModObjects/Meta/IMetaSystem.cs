using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public interface IMetaSystem
    {
        public string Identifier { get; }
        public string Name { get; }
        public string Version { get; }
        public string Description { get; }
        public MetaObject[] Objects { get; set; }
        public MetaPropUIAttribute[] PropUIAttributes { get; set; }
    }
}
