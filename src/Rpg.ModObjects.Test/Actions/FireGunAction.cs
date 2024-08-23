using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Actions
{
    public class FireGunAction : ModObjects.Actions.Action<TestGun>
    {
        [JsonConstructor] private FireGunAction() { }

        public FireGunAction(TestGun owner)
            : base(owner) { }

        public bool OnCanAct(TestGun owner, TestHuman initiator)
            => !owner.IsStateOn(nameof(AmmoEmptyState));

        public bool OnCost(Activity activity, TestGun owner, TestHuman initiator)
        {
            activity.OutcomeSet
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);

            return true;
        }

        public bool OnAct(Activity activity, TestHuman initiator, int targetDefence)
        {
            activity
                .ActivityMod("diceRoll", "Base", "1d20")
                .ActivityMod("diceRoll", initiator, x => x.MissileAttack)
                .ActivityMod("target", "Base", 10)
                .ActivityMod("target", "targetDefence", targetDefence);

            return true;
        }

        public bool OnOutcome(Activity activity, TestGun owner, TestHuman initiator, int diceRoll, int target)
        {
            activity
                .ActivityMod("damage", owner, x => x.Damage.Dice)
                .ActivityMod("damage", initiator, x => x.Dexterity.Bonus);
            
            activity.OutcomeSet
                .Add(new Permanent(), owner, x => x.Ammo.Current, -1);

            var firing = owner.CreateStateInstance(nameof(GunFiring), new SpanOfTime(0, 1))!;
            activity.OutputSets.Add(firing);

            return true;
        }
    }
}
