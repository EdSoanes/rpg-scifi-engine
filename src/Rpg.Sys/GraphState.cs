using Rpg.Sys.Components;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class GraphState<T> where T : ModObject
    {
        public T? Context { get; set; }
        public Modifier[]? Mods { get; set; }
        public Condition[]? Conditions { get; set; }
        public int Turn { get; set; }
    }
}
