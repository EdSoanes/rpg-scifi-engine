using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : State<Actor>
    {
        [JsonConstructor] private MeleeAttacked() { }

        public MeleeAttacked(Actor owner)
           : base(owner) { }
    }
}
