using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestBuffState : ModState<ModdableEntity>
    {
        public TestBuffState()
            : base("Buff")
        { }

        protected override bool ShouldApply()
        {
            return (Entity?.Melee.Roll() ?? 0) >= 10;
        }

        protected override ModSet CreateState()
        {
            return new ModSet(ModDuration.Permanent(),
                PermanentMod.Create(Entity!, x => x.Health, 10));
        }
    }
}
