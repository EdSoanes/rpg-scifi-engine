using Rpg.ModObjects.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class ScoreUIAttribute : IntegerUIAttribute
    {
        public ScoreUIAttribute()
        {
            Min = 3;
            Max = 18;
        }
    }

    public class PresenceUIAttribute : IntegerUIAttribute
    {
        public PresenceUIAttribute()
        {
            Min = 0;
            Max = 10;
        }
    }
}
