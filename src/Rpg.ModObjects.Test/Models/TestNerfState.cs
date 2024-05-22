using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.ComponentModel.DataAnnotations;

namespace Rpg.ModObjects.Tests.Models
{
    public class TestNerfState : ModState
    {
        public TestNerfState()
            => Name = "Nerf";

        protected override bool ShouldActivate()
        {
            return Graph!.GetEntity<ModdableEntity>(EntityId)!.Melee.Roll() < 1;
        }

        protected override void OnActivate(ModSet modSet)
        {
            var entity = Graph!.GetEntity<ModdableEntity>(EntityId)!;
            modSet.AddMod(entity, x => x.Health, 10, () => DiceCalculations.Minus);
        }
    }
}
