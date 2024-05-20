using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ModCmdArg
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public ModCmdArgType ArgType { get; set; }
    }
}
