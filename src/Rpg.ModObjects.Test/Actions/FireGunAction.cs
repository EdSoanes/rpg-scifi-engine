using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.ModObjects.Tests.Actions
{
    public class FireGunAction : ModObjects.Actions.Action<TestGun>
    {
        [JsonConstructor] private FireGunAction() { }

        public FireGunAction(TestGun owner)
            : base(owner) { }

        public bool OnCanAct(TestGun owner, TestHuman initiator)
            => !owner.IsStateOn(nameof(AmmoEmptyState));

        public ModSet OnCost(TestGun owner, TestHuman initiator)
        {
            return new ModSet(initiator.Id, new TurnLifecycle())
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ActionModSet OnAct(ActionInstance actionInstance, TestHuman initiator, int targetDefence)
        {
            var actionModSet = actionInstance
                .CreateActionSet()
                .DiceRoll(initiator, "Base", "d20")
                .DiceRoll(initiator, x => x.MissileAttack)
                .Target(initiator, "Base", 10)
                .Target(initiator, "TargetDefence", targetDefence);

            return actionModSet;
        }

        public ModSet[] OnOutcome(ActionInstance actionInstance, TestGun owner, TestHuman initiator, int target, int diceRoll)
        {
            var outcome = actionInstance
                .CreateOutcomeSet()
                .Outcome(initiator, owner, x => x.Damage.Dice)
                .Outcome(initiator, x => x.Dexterity.Bonus)
                .Add(new PermanentMod(), owner, x => x.Ammo.Current, -1);

            var firing = owner.CreateStateInstance(nameof(GunFiring))!;
            
            return [outcome, firing];
        }
    }
}
