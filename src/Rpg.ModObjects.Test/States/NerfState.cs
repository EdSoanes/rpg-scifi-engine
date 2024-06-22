using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class NerfState : State<ModdableEntity>
    {
        public NerfState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() < 1;

        protected override void WhenOn(ModdableEntity owner)
            => Mod(owner, x => x.Health, -10);
    }
}
