﻿using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Movement : RpgComponent
    {
        [JsonProperty] public MinMaxValue Speed { get; private set; }
        [JsonProperty] public int Acceleration { get; protected set; }
        [JsonProperty] public int Deceleration { get; protected set; }

        [JsonConstructor] private Movement() { }

        public Movement(string entityId, string name, MovementTemplate template)
            : base(entityId, name)
        {
            Speed = new MinMaxValue(entityId, nameof(Speed), template.MaxSpeed, 0);
            Acceleration = template.Acceleration;
            Deceleration = template.Deceleration;
        }

        public Movement(string entityId, string name, int maxSpeed, int acceleration, int deceleration)
            : base(entityId, name)
        {
            Speed = new MinMaxValue(entityId, nameof(Speed), maxSpeed, 0);
            Acceleration = acceleration;
            Deceleration = deceleration;
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Speed.OnBeforeTime(graph, entity);
        }
    }
}
