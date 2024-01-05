using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Health
    {
        [JsonProperty] public MaxCurrentValue Physical { get; private set; }
        [JsonProperty] public MaxCurrentValue Mental { get; private set; }
        [JsonProperty] public MaxCurrentValue Cyber { get; private set; }

        [JsonConstructor] private Health() { }

        public Health(HealthTemplate template)
        {
            Physical = new MaxCurrentValue(template.Physical, template.Physical);
            Mental = new MaxCurrentValue(template.Mental, template.Mental);
            Cyber = new MaxCurrentValue(template.Cyber, template.Cyber);
        }
    }
}
