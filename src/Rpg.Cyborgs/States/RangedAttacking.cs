using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class RangedAttacking : RpgState<Actor>
    {
        [JsonConstructor] private RangedAttacking() { IsPlayerVisible = false; }

        public RangedAttacking(Actor owner)
           : base(owner) { IsPlayerVisible = false; }
    }
}
