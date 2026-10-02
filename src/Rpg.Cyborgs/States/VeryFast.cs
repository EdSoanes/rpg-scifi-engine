using Newtonsoft.Json;
using Rpg.Experimental;

namespace Rpg.Cyborgs.States
{
    public class VeryFast : RpgState<Actor>
    {
        [JsonConstructor] private VeryFast() { }

        public VeryFast(Actor owner)
            : base(owner)
        {
            this.Add(owner, x => x.ActionPoints, 1);
        }

        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions > 10;
    }
}
