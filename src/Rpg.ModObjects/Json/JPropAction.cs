using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Json
{
    public class JPropAction
    {
        public Ops Op { get; set; }
        public string System { get; set; }
        public string EntitySetName { get; set; }
        public string TargetProp { get; set; }
        public string SourceProp { get; set; }
        public object? OldVal { get; set; }
        public object? NewVal { get; set; }

        public override string ToString()
        {
            var op = Op switch
            {
                Ops.Insert => "+",
                Ops.Update => "",
                Ops.Delete => "-"
            };

            return $"{op}{TargetProp}({OldVal})->({NewVal})";
        }
    }
}
