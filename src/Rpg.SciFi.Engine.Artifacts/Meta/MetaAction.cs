using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Meta
{
    public class MetaAction
    {
        public string Name { get; set; }
        public List<MetaActionInput> Inputs { get; set; } = new List<MetaActionInput>();
    }
}
