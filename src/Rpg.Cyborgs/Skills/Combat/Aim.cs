using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using System.Security.Cryptography.X509Certificates;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class Aim : Skill
    {
        [JsonConstructor] private Aim() { }

        public Aim(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public bool OnCanAct(Actor owner)
            => !owner.IsStateOn(nameof(Aiming)) || owner.RangedAimBonus.Value < 6;

        public bool OnCost(Activity activity, Actor owner, Actor initiator)
        {
            activity.CostSet
                .Add(initiator, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(Activity activity)
            => true;

        public bool OnOutcome(Activity activity, Actor owner)
        {
            activity.OutcomeSet
                .Add(owner, x => x.RangedAimBonus, 2);

            var aiming = owner.CreateStateInstance(nameof(Aiming));
            activity.OutputSets.Add(aiming);

            return true;
        }
    }
}
