using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AbilityAttribute : Attribute
    {
        public AbilityAttribute()
        {
        }
    }
}
