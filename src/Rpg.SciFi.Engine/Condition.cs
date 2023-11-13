using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public class Condition
    {
        public const int InfiniteRadius = -1;
        public const int TouchRadius = 0;

        public virtual string Name { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public virtual double Value { get; set; } = 0.0;
        public virtual string Unit { get; set; } = "meter";
        public virtual int Radius { get; set; } = InfiniteRadius;
    }
}
