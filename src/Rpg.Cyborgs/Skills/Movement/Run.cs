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

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => true;

        public ModSet OnCost(Actor owner, int focusPoints)
        {
            return new ModSet(owner, new TurnLifecycle())
                .Add(owner, x => x.CurrentFocusPoints, -focusPoints)
                .Add(owner, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(Actor owner)
        {
            return [new ModSet(owner, new TurnLifecycle())];
        }

        public ModSet[] OnOutcome(Actor owner)
        {
            var moving = owner.CreateStateInstance(nameof(Moving), new TurnLifecycle());
            return [moving];
        }
    }
}
