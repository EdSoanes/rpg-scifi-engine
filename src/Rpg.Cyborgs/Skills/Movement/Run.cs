using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Skills.Movement
{
    public class Run : Skill
    {
        [JsonConstructor] protected Run() { }

        public Run(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public bool CanPerform(Actor owner)
            => owner.CurrentActionPoints > 0;

        public bool Cost(ModObjects.Activities.Action action, Actor owner)
        {
            action.CostModSet.Add(owner, x => x.CurrentActionPoints, -1);
            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, Actor owner)
        {
            action.SetOutcomeState(owner, nameof(Moving), new Lifespan(0, 1));
            return true;
        }
    }
}
