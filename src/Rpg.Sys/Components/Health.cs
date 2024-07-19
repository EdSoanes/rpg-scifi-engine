using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Health : RpgComponent
    {
        [JsonProperty] public HealthValue Physical { get; private set; } = new HealthValue(nameof(Physical));
        [JsonProperty] public HealthValue Mental { get; private set; } = new HealthValue(nameof(Mental));
        [JsonProperty] public HealthValue Cyber { get; private set; } = new HealthValue(nameof(Cyber));

        [JsonConstructor] private Health() { }

        public Health(string name)
            : base(name)
        {
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Physical.OnBeforeTime(graph, entity);
            Cyber.OnBeforeTime(graph, entity);
            Mental.OnBeforeTime(graph, entity);
        }
    }
}
