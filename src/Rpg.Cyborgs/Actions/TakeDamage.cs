using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class TakeDamage : ActionTemplate<Actor>
    {
        [JsonConstructor] protected TakeDamage()
            : base() { }

        public TakeDamage(Actor owner)
            : base(owner) { }

        public bool Outcome(ModObjects.Activities.Action action, Actor owner, int damage)
        {
            var staminaInjury = owner.CurrentStaminaPoints >= damage
                ? damage
                : owner.CurrentStaminaPoints;

            if (staminaInjury > 0)
            {
                action.OutcomeModSet.Add(new Permanent(new Combine()), owner, x => x.CurrentStaminaPoints, -staminaInjury);
                action.SetProp("staminaInjury", staminaInjury);
            }

            //If there is damage over after inflicting it on stamina...
            var lifeInjury = staminaInjury < damage
                ? damage - staminaInjury
                : 0;

            if (lifeInjury > 0)
            {
                action.OutcomeModSet.Add(new Permanent(new Combine()), owner, x => x.CurrentLifePoints, -lifeInjury);
                action
                    .SetProp("lifeInjury", lifeInjury)
                    .SetOutcomeAction(owner, nameof(TakeInjury), false);
            }

            return true;
        }
    }
}
