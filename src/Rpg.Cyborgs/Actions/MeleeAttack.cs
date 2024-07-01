using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

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
            => true;

        public ModSet OnCost(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints)
        {
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter))
                .AddMod(new TurnMod(), initiator, x => x.CurrentFocusPoints, -focusPoints);
        }

        public ModSet OnAct(int actionNo, MeleeWeapon owner, Actor initiator, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));

            ActResultMod(actionNo, modSet, initiator, "Base", "2d6");
            ActResultMod(actionNo, modSet, initiator, "FocusPoints", focusPoints);

            if (abilityScore != null)
                ActResultMod(actionNo, modSet, initiator, "Ability", abilityScore.Value);
            else
                ActResultMod(actionNo, modSet, initiator, x => x.MeleeAttack);

            return modSet;
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            return Array.Empty<ModSet>();
        }
    }
}
