using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : RpgState<Actor>
    {
        [JsonConstructor] private MeleeAttacked() { IsPlayerVisible = false; }

        public MeleeAttacked(Actor owner)
           : base(owner) { IsPlayerVisible = false; }
    }
}
