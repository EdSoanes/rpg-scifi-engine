using Newtonsoft.Json;
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

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => true;

        public ModSet OnCost(TestHuman owner)
            => new ModSet(owner, new TurnLifecycle());

        public ModSet[] OnAct(TestHuman owner, int damage)
        {
            return [new ModSet(owner, new TurnLifecycle())
                .Add(new PermanentMod(), owner, x => x.Health, -damage)];
        }

        public ModSet[] OnOutcome(TestHuman owner)
            => Array.Empty<ModSet>();
    }
}
