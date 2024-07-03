using Newtonsoft.Json;
using Rpg.Cyborgs.Skills.Combat;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Actions
{
    public class RangedAttack : ModObjects.Actions.Action<RangedWeapon>
    {
        [JsonConstructor] private RangedAttack() { }

        public RangedAttack(RangedWeapon owner)
            : base(owner)
        {
        }

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => (initiator as Actor)!.Hands.Contains(owner);

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
            ActResultMod(actionNo, modSet, initiator, $"{nameof(Aim)}_{nameof(Aim.Rating)}");

            if (abilityScore != null)
                ActResultMod(actionNo, modSet, initiator, "Ability", abilityScore.Value);
            else
                ActResultMod(actionNo, modSet, initiator, x => x.RangedAttack);

            return modSet;
        }

        public ModSet[] OnOutcome(MeleeWeapon owner, Actor initiator, int diceRoll, int targetDefence)
        {
            var moving = owner.CreateStateInstance(nameof(Firing), new TimeLifecycle(TimePoints.Encounter(1)));
            var res = new List<ModSet>()
            {
                moving
            };

            return res.ToArray();
        }
    }
}
