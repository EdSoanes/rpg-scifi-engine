using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class MeleeAttacked : State<Actor>
    {
        [JsonConstructor] private MeleeAttacked() { }

        public MeleeAttacked(Actor owner)
           : base(owner) { }
    }
}
