using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.ModObjects.Tests.Actions
{
    [Action(Required = true)]
    public class TakeDamageAction : ModObjects.Actions.Action<TestHuman>
    {
        [JsonConstructor] private TakeDamageAction() { }

        public TakeDamageAction(TestHuman owner)
            : base(owner) { }

        public bool OnCanAct(TestHuman owner)
            => true;

        public ModSet OnCost(TestHuman owner)
            => new ModSet(owner.Id, new TurnLifecycle(), "Cost");

        public ActionModSet OnAct(ActionInstance actionInstance, TestHuman owner, int damage)
        {
            return actionInstance.CreateActionSet();
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, TestHuman owner, int damage)
        {
            var outcome = actionInstance
                .CreateOutcomeSet()
                .Add(new PermanentMod(), owner, x => x.Health, -damage);

            return [outcome];
        }
    }
}
