using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Base : Permanent
    {
        public Base()
            : base(nameof(Base))
        { }

        public Base(RpgObject target, string targetProp, Dice Value)
            : base(nameof(Base))
                => Set(target, targetProp, Value);

        public Base(string name, RpgObject target, string targetProp, Dice Value)
            : base(name)
                => Set(target, targetProp, Value);
    }
}
