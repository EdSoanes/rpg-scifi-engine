using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeParry : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private MeleeParry() { }

        public MeleeParry(Actor owner)
            : base(owner)
        {
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

        public ModSet[] OnOutcome(int actionNo, Actor owner, int diceRoll, int target, int damage)
        {
            var parrying = owner.GetState(nameof(Parrying))!.CreateInstance(new TurnLifecycle(1, 1));
            var damageSet = new ModSet(owner.Id, new TurnLifecycle());
            OutcomeMod(actionNo, damageSet, owner, "Damage", damage);

            //If successful parry...
            if (diceRoll >= target)
                OutcomeMod(actionNo, damageSet, owner, x => -x.Strength);

            return [parrying];
        }
    }
}
