using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Values;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Components.Values;
using System.Xml.Linq;

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

        public int CalculateMoveDistance(Actor actor, int distance)
        {
            var moveDistance = actor.Movement.Speed.Max - (actor.Movement.Speed.Current + distance);
            if (moveDistance < 0)
                moveDistance = 0;

            return moveDistance;
        }

        public int CalculateMoveCost(Actor actor, int distance)
        {
            var moveCost = actor.Movement.Speed.Max - (actor.Movement.Speed.Current + distance);
            if (moveCost < 0)
                moveCost = 0;

            return moveCost;
        }

    }
}
