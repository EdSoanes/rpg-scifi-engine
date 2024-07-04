using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.Cyborgs.Actions
{
    public class MeleeAttack : ModObjects.Actions.Action<MeleeWeapon>
    {
        [JsonConstructor] private MeleeAttack() { }

        public MeleeAttack(MeleeWeapon owner)
            : base(owner)
        {
        }

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => (initiator as Actor)!.Hands.Contains(owner) && (initiator as Actor)!.CurrentActions > 0;

        public ModSet OnCost(Actor initiator, int focusPoints)
        {
            return new ModSet(initiator, new TurnLifecycle())
                .Add(initiator, x => x.CurrentFocusPoints, -focusPoints)
                .Add(initiator, x => x.CurrentActions, -1);
        }

        public ModSet[] OnAct(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(initiator, new TurnLifecycle());

            ActResult(actionNo, modSet, initiator, "Base", "2d6");
            ActResult(actionNo, modSet, initiator, "FocusPoints", focusPoints);

            if (abilityScore != null)
                ActResult(actionNo, modSet, initiator, "Ability", abilityScore.Value);
            else
                ActResult(actionNo, modSet, initiator, x => x.MeleeAttack);

            var attacking = initiator.CreateStateInstance(nameof(MeleeAttacking), new TurnLifecycle(1));
            return [modSet, attacking];
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            return Array.Empty<ModSet>();
        }
    }
}
