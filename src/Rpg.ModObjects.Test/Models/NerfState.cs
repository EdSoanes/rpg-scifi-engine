using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Tests.Models
{
    public class NerfState : StateModification<ModdableEntity>
    {
        [JsonConstructor] private NerfState() { }

        public NerfState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() < 1;

        protected override void WhenOn(ModdableEntity owner)
            => Mod(owner, x => x.Health, -10);
    }
}
