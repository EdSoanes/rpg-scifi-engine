using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacking : State<Actor>
    {
        [JsonConstructor] private MeleeAttacking() { }

        public MeleeAttacking(Actor owner)
           : base(owner) { }
    }
}
