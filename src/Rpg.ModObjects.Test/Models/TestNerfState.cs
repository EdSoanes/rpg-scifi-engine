using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestNerfState : ModState<ModdableEntity>
    {
        public TestNerfState()
            : base("Nerf")
        { }

        protected override bool ShouldApply()
        {
            return Entity!.Melee.Roll() < 1;
        }

        protected override void OnCreateState(ModSet<ModdableEntity> modSet)
        {
            modSet.Add(Entity!, x => x.Health, 10, () => DiceCalculations.Minus);
        }
    }
}
