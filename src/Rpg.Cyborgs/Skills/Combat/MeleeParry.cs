using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class MeleeParry : Skill
    {
        [JsonConstructor] private MeleeParry() { }

        public MeleeParry(Actor owner)
            : base(owner) 
        {
            IsIntrinsic = true;
        }

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => !initiator.IsStateOn(nameof(Parrying));

        public ModSet OnCost(Actor owner, Actor initiator, int focusPoints)
        {
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter))
                .AddMod(new TurnMod(), owner, x => x.CurrentFocusPoints, -focusPoints)
                .AddMod(new TurnMod(1, 1), initiator, x => x.Actions, -1);
        }

        public ModSet OnAct(int actionNo, Actor owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));

            ActResultMod(actionNo, modSet, initiator, "Base", "2d6");
            ActResultMod(actionNo, modSet, initiator, "FocusPoints", focusPoints);

            if (abilityScore != null)
                ActResultMod(actionNo, modSet, initiator, "AbilityScore", abilityScore.Value);
            else
                ActResultMod(actionNo, modSet, initiator, x => x.MeleeAttack);

            return modSet;
        }

        public ModSet[] OnOutcome(Actor owner, int diceRoll, int targetDefence)
        {
            var parrying = owner.CreateStateInstance(nameof(Parrying), new TimeLifecycle(TimePoints.Encounter(1)));
            var res = new List<ModSet>()
            {
                parrying
            };

            return res.ToArray();
        }
    }
}
