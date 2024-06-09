using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Health : RpgComponent
    {
        [JsonProperty] public MinMaxValue Physical { get; private set; }
        [JsonProperty] public MinMaxValue Mental { get; private set; }
        [JsonProperty] public MinMaxValue Cyber { get; private set; }

        [JsonConstructor] private Health() { }

        public Health(string entityId, string name, HealthTemplate template)
            : base(entityId, name)
        {
            Physical = new MinMaxValue(entityId, nameof(Physical), template.Physical);
            Mental = new MinMaxValue(entityId, nameof(Mental), template.Mental);
            Cyber = new MinMaxValue(entityId, nameof(Cyber), template.Cyber);
        }
    }
}
