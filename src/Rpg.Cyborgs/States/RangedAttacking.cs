using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class RangedAttacking : State<Actor>
    {
        [JsonConstructor] private RangedAttacking() { }

        public RangedAttacking(Actor owner)
           : base(owner) { }
    }
}
