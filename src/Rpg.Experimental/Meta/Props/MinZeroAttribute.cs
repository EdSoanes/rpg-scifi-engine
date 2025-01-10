using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Meta.Props
{
    public class MinZeroAttribute : IntegerAttribute
    {
        public MinZeroAttribute()
            : base()
        {
            Min = 0;
        }
    }
}
