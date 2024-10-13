using Rpg.ModObjects.States;
using Newtonsoft.Json;

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
