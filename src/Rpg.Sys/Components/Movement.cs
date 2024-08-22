using Newtonsoft.Json;
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

        public Movement(string name, MovementTemplate template)
            : base(name)
        {
            Speed = new MinMaxValue(nameof(Speed), template.MaxSpeed, 0);
            Acceleration = template.Acceleration;
            Deceleration = template.Deceleration;
        }

        public Movement(string name, int maxSpeed, int acceleration, int deceleration)
            : base(name)
        {
            Speed = new MinMaxValue(nameof(Speed), maxSpeed, 0);
            Acceleration = acceleration;
            Deceleration = deceleration;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Speed.OnCreating(graph, entity);
        }
    }
}
