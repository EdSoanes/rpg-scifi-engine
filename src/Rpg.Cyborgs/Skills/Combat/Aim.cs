using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class Aim : Skill
    {
        [JsonConstructor] protected Aim() { }

        public Aim(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public override void OnCreating(RpgGraph graph, RpgEntity owner)
        {
            base.OnCreating(graph, owner);
            owner.AddMods(new Initial(owner.Id, RatingProp, 1));
        }

        public bool OnCanAct(Actor owner)
            => !owner.IsStateOn(nameof(Aiming)) || owner.RangedAimBonus.Value < 6;

        public bool Cost(ModObjects.Activities.Action action, Actor initiator)
        {
            action.CostModSet.Add(initiator, x => x.CurrentActionPoints, -1);
            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, Actor owner)
        {
            action.OutcomeModSet.Add(new Turn(), owner, x => x.RangedAimBonus, 2);
            action.SetOutcomeState(owner, nameof(Aiming), new Lifespan(0, 1));

            return true;
        }
    }
}
