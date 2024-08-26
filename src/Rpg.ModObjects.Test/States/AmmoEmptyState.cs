using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class AmmoEmptyState : State<TestGun>
    {
        [JsonConstructor] private AmmoEmptyState() { }

        public AmmoEmptyState(TestGun owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(TestGun owner)
            => owner.Ammo.Current <= 0;

        protected override void OnFillStateSet(StateModSet modSet, TestGun owner)
        {
        }
    }
}
