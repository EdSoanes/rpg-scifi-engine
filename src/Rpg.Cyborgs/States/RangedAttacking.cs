using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class RangedAttacking : State<Actor>
    {
        [JsonConstructor] private RangedAttacking() { }

        public RangedAttacking(Actor owner)
           : base(owner) { }
    }
}
