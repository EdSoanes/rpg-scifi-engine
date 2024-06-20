using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Tests.Models
{
    public class BuffState : StateModification<ModdableEntity>
    {
        [JsonConstructor] private BuffState() { }

        public BuffState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() >= 10;

        protected override void WhenOn(ModdableEntity owner)
            => Mod(owner, x => x.Health, 10);
    }
}
