using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public class MetaActionInput
    {
        public string Name { get; set; }
        public string BindsTo { get; set; }
        public InputSource InputSource { get; set; }
    }
}
