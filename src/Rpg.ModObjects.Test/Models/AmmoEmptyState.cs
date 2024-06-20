using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Tests.Models
{
    public class AmmoEmptyState : StateModification<TestGun>
    {
        [JsonConstructor] private AmmoEmptyState() { }

        public AmmoEmptyState(TestGun owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(TestGun owner)
            => owner.Ammo.Current <= 0;

        protected override void WhenOn(TestGun owner) { }
    }
}
