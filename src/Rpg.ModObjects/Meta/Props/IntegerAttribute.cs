using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Props
{
    public class IntegerAttribute : MetaPropAttribute
    {
        public string Unit { get; protected set; } = nameof(Int32);
        public int Min { get; set; } = int.MinValue;
        public int Max { get; set; } = int.MaxValue;

        public IntegerAttribute()
            : base()
        {
            Editor = EditorType.Int32;
            Returns = ReturnType.Int32;
        }
    }
}
