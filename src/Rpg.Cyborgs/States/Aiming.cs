using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Aiming : RpgState<Actor>
    {
        [JsonConstructor] private Aiming() { IsPlayerVisible = false; }

        public Aiming(Actor owner) 
            : base(owner) { IsPlayerVisible = false; }
    }
}
