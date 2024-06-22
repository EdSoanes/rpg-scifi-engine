using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class BuffState : State<ModdableEntity>
    {
        public BuffState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() >= 10;

        protected override void WhenOn(ModdableEntity owner)
            => Mod(owner, x => x.Health, 10);
    }
}
