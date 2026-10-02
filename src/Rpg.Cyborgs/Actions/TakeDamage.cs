using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Actions
{
    public class TakeDamage : RpgAction<Actor>
    {
        [JsonConstructor] protected TakeDamage()
            : base() { }

        public TakeDamage(Actor owner)
            : base(owner) { }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph
                .CreateVirtualProperty(activityAction, "staminaInjury")
                .CreateVirtualProperty(activityAction, "lifeInjury");
        }

        public bool Outcome(RpgGraph graph, RpgActivityAction activityAction, Actor owner, int damage)
        {
            graph
                .Reset(activityAction, "staminaInjury")
                .Reset(activityAction, "lifeInjury");

            var staminaInjury = owner.CurrentStaminaPoints >= damage
                ? damage
                : owner.CurrentStaminaPoints;

            if (staminaInjury > 0)
            {
                //Damage is permanent, it must outlive the activity
                activityAction.Result
                    .Add(new Combine()
                        .SetTarget(owner, x => x.CurrentStaminaPoints)
                        .SetSource(-staminaInjury));

                graph.Add(new Standard(), activityAction, "staminaInjury", staminaInjury);
            }

            //If there is damage over after inflicting it on stamina...
            var lifeInjury = staminaInjury < damage
                ? damage - staminaInjury
                : 0;

            if (lifeInjury > 0)
            {
                activityAction.Result
                    .Add(new Combine()
                        .SetTarget(owner, x => x.CurrentLifePoints)
                        .SetSource(-lifeInjury));

                graph.Add(new Standard(), activityAction, "lifeInjury", lifeInjury);

                activityAction
                    .SetOutcomeAction(owner, nameof(TakeInjury), false);
            }

            return true;
        }
    }
}
