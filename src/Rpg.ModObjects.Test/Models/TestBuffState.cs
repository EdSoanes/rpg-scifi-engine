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
            return Entity!.Melee.Roll() >= 10;
        }

        protected override void OnCreateState(ModSet<ModdableEntity> modSet)
        {
            modSet.Add(Entity!, x => x.Health, 10);
        }
    }
}
