using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public abstract class GunTemplate
    {
        public int Range { get; set; }
        public int Attack { get; set; }
        public Dice Impact { get; set; }
        public Dice Pierce { get; set; }
        public Dice Blast { get; set; }
        public Dice Burn { get; set; }
        public Dice Energy { get; set; }
    }
}
