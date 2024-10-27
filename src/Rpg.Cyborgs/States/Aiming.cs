using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Aiming : State<Actor>
    {
        [JsonConstructor] private Aiming() { IsPlayerVisible = false; }

        public Aiming(Actor owner) 
            : base(owner) { IsPlayerVisible = false; }
    }
}
