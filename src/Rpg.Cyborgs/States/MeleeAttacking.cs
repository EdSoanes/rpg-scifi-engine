using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacking : State<Actor>
    {
        [JsonConstructor] private MeleeAttacking() { IsPlayerVisible = false; }

        public MeleeAttacking(Actor owner)
           : base(owner) { IsPlayerVisible = false; }
    }
}
