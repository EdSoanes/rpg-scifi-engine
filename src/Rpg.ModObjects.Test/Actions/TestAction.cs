using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;

namespace Rpg.ModObjects.Tests.Actions
{
    public class TestAction : ModObjects.Actions.Action<ModdableEntity>
    {
        [JsonConstructor] private TestAction() { }

        public TestAction(ModdableEntity owner)
            : base(owner) { }


        public bool OnCanAct(RpgActivity activity, ModdableEntity owner)
            => true;

        public bool OnCost(RpgActivity activity, ModdableEntity owner, TestHuman initiator)
        {
            activity.OutcomeSet
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);

            return true;
        }

        public bool OnAct(RpgActivity activity, ModdableEntity owner, TestHuman initiator, int target)
            => true;

        public bool OnOutcome(RpgActivity activity, ModdableEntity owner, TestHuman initiator, int diceRoll)
        {
            var testing = owner.CreateStateInstance(nameof(Testing))!;
            activity.OutputSets.Add(testing);

            return true;
        }
    }
}
