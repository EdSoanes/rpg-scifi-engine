using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;

namespace Rpg.ModObjects.Tests.Actions
{
    public class FireGunAction : ModObjects.Actions.Action<TestGun>
    {
        [JsonConstructor] private FireGunAction() { }

        public FireGunAction(TestGun owner)
            : base(owner) { }

        public bool OnCanAct(TestGun owner, TestHuman initiator)
            => !owner.IsStateOn(nameof(AmmoEmptyState));

        public bool OnCost(RpgActivity activity, TestGun owner, TestHuman initiator)
        {
            activity.OutcomeSet
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);

            return true;
        }

        public bool OnAct(RpgActivity activity, TestHuman initiator, int targetDefence)
        {
            activity
                .ActionMod("diceRoll", "Base", "1d20")
                .ActionMod("diceRoll", initiator, x => x.MissileAttack)
                .ActionMod("target", "Base", 10)
                .ActionMod("target", "targetDefence", targetDefence);

            return true;
        }

        public bool OnOutcome(RpgActivity activity, TestGun owner, TestHuman initiator, int diceRoll, int target)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage.Dice)
                .ActivityMod("damage", initiator, x => x.Dexterity.Bonus);
            
            activity.OutcomeSet
                .Add(new PermanentMod(), owner, x => x.Ammo.Current, -1);

            var firing = owner.CreateStateInstance(nameof(GunFiring))!;
            activity.OutputSets.Add(firing);

            return true;
        }
    }
}
