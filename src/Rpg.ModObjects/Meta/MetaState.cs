using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaState
    {
        public string Name { get; set; }
        public string? ActiveWhen { get; set; }

        public MetaState(string name, string? activeWhen)
        {
            Name = name;
            ActiveWhen = activeWhen;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
