using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Actions
{
    public class TestAction : ModObjects.Actions.Action<ModdableEntity>
    {
        [JsonConstructor] private TestAction() { }

        public TestAction(ModdableEntity owner)
            : base(owner) { }


        public bool OnCanAct(Activity activity, ModdableEntity owner)
            => true;

        public bool OnCost(Activity activity, ModdableEntity owner, TestHuman initiator)
        {
            activity.OutcomeSet
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);

            return true;
        }

        public bool OnAct(Activity activity, ModdableEntity owner, TestHuman initiator, int target)
            => true;

        public bool OnOutcome(Activity activity, ModdableEntity owner, TestHuman initiator, int diceRoll)
        {
            var testing = owner.CreateStateInstance(nameof(Testing), new SpanOfTime(0, 1))!;
            activity.OutputSets.Add(testing);

            return true;
        }
    }
}
