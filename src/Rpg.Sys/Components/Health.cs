using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Health : RpgEntityComponent
    {
        [JsonProperty] public MaxCurrentValue Physical { get; private set; }
        [JsonProperty] public MaxCurrentValue Mental { get; private set; }
        [JsonProperty] public MaxCurrentValue Cyber { get; private set; }

        [JsonConstructor] private Health() { }

        public Health(string entityId, string name, HealthTemplate template)
            : base(entityId, name)
        {
            Physical = new MaxCurrentValue(entityId, nameof(Physical), template.Physical);
            Mental = new MaxCurrentValue(entityId, nameof(Mental), template.Mental);
            Cyber = new MaxCurrentValue(entityId, nameof(Cyber), template.Cyber);
        }
    }
}
