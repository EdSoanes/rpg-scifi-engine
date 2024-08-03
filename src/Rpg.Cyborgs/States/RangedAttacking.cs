using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class RangedAttacking : State<Actor>
    {
        [JsonConstructor] private RangedAttacking() { }

        public RangedAttacking(Actor owner)
           : base(owner) { }
    }
}
