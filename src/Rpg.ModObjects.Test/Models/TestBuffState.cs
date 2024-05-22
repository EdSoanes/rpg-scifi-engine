using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestBuffState : ModState
    {
        public TestBuffState()
            => Name = "Buff";
        
        protected override bool ShouldActivate()
            => Graph!.GetEntity<ModdableEntity>(EntityId)!.Melee.Roll() >= 10;
        

        protected override void OnActivate(ModSet modSet)
        {
            var entity = Graph!.GetEntity<ModdableEntity>(EntityId)!;
            modSet.AddMod(entity, x => x.Health, 10);
        }
    }
}
