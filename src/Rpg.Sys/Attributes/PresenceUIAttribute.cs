using Rpg.ModObjects.Meta.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Attributes
{
    public class PresenceUIAttribute : IntegerUIAttribute
    {
        public PresenceUIAttribute()
        {
            Min = 0;
            Max = 10;
        }
    }
}
