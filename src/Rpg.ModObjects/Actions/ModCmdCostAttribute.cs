using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModCmdCostAttribute : Attribute
    {
        public int Physical { get; private set; }
        public int Mental { get; private set; }

        public ModCmdCostAttribute()
        {
            Physical = 0;
            Mental = 0;
        }

        public ModCmdCostAttribute(int physical, int mental)
        {
            Physical = physical;
            Mental = mental;
        }
    }
}
