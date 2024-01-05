using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class HealthTemplate
    {
        public int Physical { get; set; } = 1;
        public int Mental { get; set; }
        public int Cyber { get; set; }
    }
}
