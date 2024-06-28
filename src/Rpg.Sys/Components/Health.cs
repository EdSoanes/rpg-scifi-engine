using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Health : RpgComponent
    {
        [JsonProperty] public HealthValue Physical { get; private set; }
        [JsonProperty] public HealthValue Mental { get; private set; }
        [JsonProperty] public HealthValue Cyber { get; private set; }

        [JsonConstructor] private Health() { }

        public Health(string entityId, string name)
            : base(entityId, name)
        {
            Physical = new HealthValue(entityId, nameof(Physical));
            Mental = new HealthValue(entityId, nameof(Mental));
            Cyber = new HealthValue(entityId, nameof(Cyber));
        }
    }
}
