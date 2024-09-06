using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacking : State<Actor>
    {
        [JsonConstructor] private MeleeAttacking() { }

        public MeleeAttacking(Actor owner)
           : base(owner) { }
    }
}
