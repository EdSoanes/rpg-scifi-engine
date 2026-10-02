using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Exhausted : RpgState<Actor>
    {
        [JsonConstructor] private Exhausted() { }

        public Exhausted(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.CurrentStaminaPoints == 0;
    }
}
