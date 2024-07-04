using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
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
            => owner.CurrentActions > 0;

        public ModSet OnCost(Actor owner, int focusPoints)
        {
            return new ModSet(owner.Id, new TurnLifecycle())
                .Add(owner, x => x.CurrentFocusPoints, -focusPoints)
                .Add(owner, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(Actor owner)
        {
            return [new ModSet(owner.Id, new TurnLifecycle())];
        }

        public ModSet[] OnOutcome(Actor owner)
        {
            var moving = owner.GetState(nameof(Moving))!.CreateInstance(new TurnLifecycle());
            return [moving];
        }
    }
}
