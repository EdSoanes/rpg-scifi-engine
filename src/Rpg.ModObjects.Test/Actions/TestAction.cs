using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.ModObjects.Tests.Actions
{
    public class TestAction : ModObjects.Actions.Action<ModdableEntity>
    {
        [JsonConstructor] private TestAction() { }

        public TestAction(ModdableEntity owner)
            : base(owner) { }


        public bool OnCanAct(ModdableEntity owner)
            => true;

        public ModSet OnCost(ModdableEntity owner, TestHuman initiator)
        {
            return new ModSet(owner.Id, new TurnLifecycle())
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, ModdableEntity owner, TestHuman initiator, int target)
        {
            return actionInstance.CreateActionSet();
        }

        public ModSet[] OnOutcome(ModdableEntity owner, TestHuman initiator, int diceRoll)
        {
            var testing = owner.CreateStateInstance(nameof(Testing))!;
            return [testing];
        }
    }
}
