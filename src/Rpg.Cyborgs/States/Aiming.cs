using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Aiming : State<Actor>
    {
        [JsonConstructor] private Aiming() { }

        public Aiming(Actor owner) 
            : base(owner) { }
    }
}
