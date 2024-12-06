using Rpg.ModObjects.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Server.Ops
{
    public class OverrideBaseValue
    {
        public PropRef PropRef {  get; set; }
        public int OverrideValue { get; set; }
    }
}
