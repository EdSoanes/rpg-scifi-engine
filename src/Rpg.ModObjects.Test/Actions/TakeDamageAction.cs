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

        public bool OnCanAct(TestHuman owner)
            => true;

        public ModSet OnCost(TestHuman owner)
            => new ModSet(owner.Id, new TurnLifecycle());

        public ModSet[] OnAct(TestHuman owner, int damage)
        {
            return [new ModSet(owner.Id, new TurnLifecycle())
                .Add(new PermanentMod(), owner, x => x.Health, -damage)];
        }

        public ModSet[] OnOutcome(TestHuman owner)
            => Array.Empty<ModSet>();
    }
}
