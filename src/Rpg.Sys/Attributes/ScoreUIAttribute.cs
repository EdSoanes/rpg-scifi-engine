using Rpg.ModObjects.Meta.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Attributes
{
    public class ScoreUIAttribute : IntegerUIAttribute
    {
        public ScoreUIAttribute()
        {
            Min = 3;
            Max = 18;
        }
    }
}
