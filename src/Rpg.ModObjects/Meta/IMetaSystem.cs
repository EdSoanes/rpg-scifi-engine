using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public interface IMetaSystem
    {
        string Identifier { get; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
        MetaObj[] Objects { get; set; }
        MetaPropUIAttribute[] PropUIAttributes { get; set; }
    }
}
