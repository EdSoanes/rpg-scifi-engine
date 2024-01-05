using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class EntityGraphState<T> where T : ModdableObject
    {
        public T? Context { get; set; }
        public ModStore? Mods { get; set; }
    }
}
