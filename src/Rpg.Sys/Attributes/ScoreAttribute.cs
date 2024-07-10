using Rpg.ModObjects.Meta.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Attributes
{
    public class ScoreAttribute : IntegerAttribute
    {
        public ScoreAttribute()
            : base()
        {
            Min = 3;
            Max = 18;
        }
    }
}
