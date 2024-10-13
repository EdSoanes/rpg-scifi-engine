using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : State<Actor>
    {
        [JsonConstructor] private MeleeAttacked() { }

        public MeleeAttacked(Actor owner)
           : base(owner) { }
    }
}
