using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class GraphState<T> where T : ModdableObject
    {
        public T? Context { get; set; }
        public ModStore? Mods { get; set; }
    }
}
