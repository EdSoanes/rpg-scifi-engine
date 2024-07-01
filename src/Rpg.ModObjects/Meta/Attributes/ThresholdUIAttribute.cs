using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Attributes
{
    public abstract class ThresholdUIAttribute : IntegerUIAttribute
    {
        public ThresholdUIAttribute(int min, int max)
            : base()
        {
            Min = min;
            Max = max;
            ReturnType = nameof(Int32);
        }
    }
}
