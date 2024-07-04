using Newtonsoft.Json;
using Rpg.Cyborgs.Skills.Combat;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Lifecycles;
using Rpg.ModObjects.Time.Templates;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : ModObjects.Actions.Action<RangedWeapon>
    {
        [JsonConstructor] private RangedAttack() { }

        public RangedAttack(RangedWeapon owner)
            : base(owner)
        {
        }

        public bool OnCanAct(MeleeWeapon owner, Actor initiator)
            => initiator.Hands.Contains(owner) && initiator.CurrentActions > 0;

        public ModSet OnCost(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(initiator, x => x.CurrentFocusPoints, -focusPoints)
                .Add(initiator, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(initiator.Id, new TurnLifecycle());

            ActResult(actionNo, modSet, initiator, "Base", "2d6");
            ActResult(actionNo, modSet, initiator, "FocusPoints", focusPoints);
            ActResult(actionNo, modSet, initiator, $"{nameof(Aim)}_{nameof(Aim.Rating)}");

            if (abilityScore != null)
                ActResult(actionNo, modSet, initiator, "Ability", abilityScore.Value);
            else
                ActResult(actionNo, modSet, initiator, x => x.RangedAttack);

            return [modSet];
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            var moving = owner.GetState(nameof(Firing))!.CreateInstance(new TurnLifecycle());
            return [moving];
        }
    }
}
