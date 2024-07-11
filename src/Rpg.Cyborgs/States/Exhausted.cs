using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Exhausted : State<Actor>
    {
        [JsonConstructor] private Exhausted() { }

        public Exhausted(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.CurrentStaminaPoints == 0;
    }
}
