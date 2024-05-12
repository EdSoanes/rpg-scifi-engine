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
            return (Entity?.Melee.Roll() ?? 0) < 1;
        }

        protected override ModSet CreateState()
        {
            return new ModSet(ModDuration.Permanent(),
                PermanentMod.Create(Entity!, x => x.Health, 10, () => DiceCalculations.Minus));
        }
    }
}
