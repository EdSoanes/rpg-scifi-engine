using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ModifiableAttribute : Attribute
    {
        public ModifiableAttribute()
        {
        }
    }
}
