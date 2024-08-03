using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.Actions
{
    [Action(Required = true)]
    public class TakeDamageAction : ModObjects.Actions.Action<TestHuman>
    {
        [JsonConstructor] private TakeDamageAction() { }

        public TakeDamageAction(TestHuman owner)
            : base(owner) { }

        public bool OnCanAct(RpgActivity activity, TestHuman owner)
            => true;

        public bool OnCost(RpgActivity activity, TestHuman owner)
            => true;

        public bool OnAct(RpgActivity activity, TestHuman owner, int damage)
        {
            activity
                .ActivityMod("damage", "Damage", damage);

            return true;
        }

        public bool OnOutcome(RpgActivity activity, TestHuman owner, int damage)
        {
            activity
                .ActivityResultMod("damage", "Result", damage);

            activity.OutcomeSet
                .Add(new PermanentMod(), owner, x => x.Health, -damage);

            return true;
        }
    }
}
