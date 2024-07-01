using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Attributes
{
    public class IntegerUIAttribute : MetaPropUIAttribute
    {
        public string Unit { get; protected set; } = nameof(Int32);
        public int Min { get; protected set; } = int.MinValue;
        public int Max { get; protected set; } = int.MaxValue;

        public IntegerUIAttribute()
        {
            ReturnType = nameof(Int32);
        }
    }
}
