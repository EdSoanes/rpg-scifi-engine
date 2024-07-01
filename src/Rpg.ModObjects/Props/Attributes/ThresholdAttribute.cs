using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Props.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ThresholdAttribute : Attribute
    {
        public int Min { get; set; } = int.MinValue;
        public int Max { get; set; } = int.MaxValue;

        public ThresholdAttribute()
        { }

        public ThresholdAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
