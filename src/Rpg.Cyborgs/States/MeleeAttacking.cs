using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacking : RpgState<Actor>
    {
        [JsonConstructor] private MeleeAttacking() { IsPlayerVisible = false; }

        public MeleeAttacking(Actor owner)
           : base(owner) { IsPlayerVisible = false; }
    }
}
