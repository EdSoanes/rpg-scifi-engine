using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Meta.Props
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ThresholdAttribute : IntegerAttribute
    {
        public ThresholdAttribute()
        {
        }
    }
}
