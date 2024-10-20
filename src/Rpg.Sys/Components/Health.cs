﻿using Rpg.ModObjects;
using Rpg.Sys.Components.Values;
using Newtonsoft.Json;

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

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Physical.OnCreating(graph, entity);
            Cyber.OnCreating(graph, entity);
            Mental.OnCreating(graph, entity);
        }
    }
}
