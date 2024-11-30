using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.Attributes
{
    public class ArgDefaultAttribute : ArgAttribute
    {
        public object Value { get; set; }

        public ArgDefaultAttribute()
            : base()
        {
        }
    }
}
