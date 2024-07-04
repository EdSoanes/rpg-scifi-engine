using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;

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

        public bool OnCanAct(Actor owner)
            => !owner.IsStateOn(nameof(Parrying));

        public ModSet OnCost(Actor owner, Actor initiator, int focusPoints)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(owner, x => x.CurrentFocusPoints, -focusPoints)
                .Add(new TurnMod(1, 1), initiator, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(int actionNo, Actor owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(initiator.Id, new TurnLifecycle());

            ActResult(actionNo, modSet, initiator, "Base", "2d6");
            ActResult(actionNo, modSet, initiator, "FocusPoints", focusPoints);

            if (abilityScore != null)
                ActResult(actionNo, modSet, initiator, "AbilityScore", abilityScore.Value);
            else
                ActResult(actionNo, modSet, initiator, x => x.MeleeAttack);

            return [modSet];
        }

        public ModSet[] OnOutcome(Actor owner, int diceRoll, int targetDefence)
        {
            var parrying = owner.GetState(nameof(Parrying))!.CreateInstance(new TurnLifecycle());
            return [parrying];
        }
    }
}
