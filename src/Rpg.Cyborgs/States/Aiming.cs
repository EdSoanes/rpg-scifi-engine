using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class Aiming : State<Actor>
    {
        [JsonConstructor] private Aiming() { }

        public Aiming(Actor owner) 
            : base(owner) { }
    }
}
