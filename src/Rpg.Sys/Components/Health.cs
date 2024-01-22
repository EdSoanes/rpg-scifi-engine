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
            Physical = new MaxCurrentValue(nameof(Physical), template.Physical, template.Physical);
            Mental = new MaxCurrentValue(nameof(Mental), template.Mental, template.Mental);
            Cyber = new MaxCurrentValue(nameof(Cyber), template.Cyber, template.Cyber);
        }
    }
}
