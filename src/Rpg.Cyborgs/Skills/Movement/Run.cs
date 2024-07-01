using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

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
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter))
                .AddMod(new TurnMod(), owner, x => x.CurrentFocusPoints, -focusPoints);
        }

        public ModSet OnAct(Actor owner)
        {
            return new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));
        }

        public ModSet[] OnOutcome(Actor owner)
        {
            var moving = owner.CreateStateInstance(nameof(Moving), new TimeLifecycle(TimePoints.Encounter(1)));
            var res = new List<ModSet>()
            {
                moving
            };

            return res.ToArray();
        }
    }
}
