using Newtonsoft.Json;
using Rpg.Cyborgs.States;
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

        public ModSet OnCost(Actor owner, int focusPoints)
        {
            return new ModSet(owner.Id, new TurnLifecycle())
                .Add(owner, x => x.CurrentFocusPoints, -focusPoints)
                .Add(owner, x => x.CurrentActionPoints, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, Actor owner)
            => actionInstance.CreateActionSet();

        public ModSet[] OnOutcome(Actor owner)
        {
            var moving = owner.GetState(nameof(Moving))!.CreateInstance(new TurnLifecycle());
            return [moving];
        }
    }
}
