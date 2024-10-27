using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : State<Actor>
    {
        [JsonConstructor] private MeleeAttacked() { IsPlayerVisible = false; }

        public MeleeAttacked(Actor owner)
           : base(owner) { IsPlayerVisible = false; }
    }
}
