using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Skills.Movement
{
    public class Run : Skill
    {
        [JsonConstructor] private Run() { }

        public Run(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public bool OnCanAct(Actor owner)
            => owner.CurrentActionPoints > 0;

        public bool OnCost(RpgActivity activity, Actor owner)
        {
            activity.CostSet
                .Add(owner, x => x.CurrentActionPoints, -1);

            return true;
        }

        public bool OnAct(ActionInstance actionInstance, Actor owner)
            => true;

        public bool OnOutcome(RpgActivity activity, Actor owner)
        {
            var moving = owner.GetState(nameof(Moving))!.CreateInstance(new TurnLifecycle());
            activity.OutputSets.Add(moving);

            return true;
        }
    }
}
