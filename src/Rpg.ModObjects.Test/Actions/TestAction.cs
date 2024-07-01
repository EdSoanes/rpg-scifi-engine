using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Actions
{
    public class TestAction : ModObjects.Actions.Action<ModdableEntity>
    {
        [JsonConstructor] private TestAction() { }

        public TestAction(ModdableEntity owner)
            : base(owner) { }

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => true;

        public ModSet OnCost(ModdableEntity owner, TestHuman initiator)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ModSet OnAct(ModdableEntity owner, TestHuman initiator, int target)
        {
            return new ModSet(owner);
        }

        public ModSet[] OnOutcome(ModdableEntity owner, TestHuman initiator, int diceRoll)
        {
            var testing = owner.CreateStateInstance(nameof(TestAction), new TimeLifecycle(TimePoints.Encounter(1)));
            var res = new List<ModSet>
            {
                testing
            };

            return res.ToArray();
        }


    }
}
