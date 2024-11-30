using Rpg.Core.Tests.Models;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.Core.Tests
{
    public class Attack : ActionTemplate<TestWeapon>
    {
        public Attack(TestWeapon owner)
            : base(owner) { }

        public bool CanPerform(TestWeapon owner, TestPerson initiator)
            => initiator.Hands.Contains(owner) && !owner.IsStateOn(nameof(WeaponDamaged));

        public bool Perform(ModObjects.Activities.Action action, TestWeapon owner, TestPerson initiator, int targetDefence)
        {
            action
                .ResetProp("diceRoll", "targetDefence")
                .SetProp("diceRoll", "1d20")
                .SetProp("diceRoll", owner, x => x.HitBonus)
                .SetProp("diceRoll", initiator, x => x.StrengthBonus)
                .SetProp("targetDefence", targetDefence);

            return true;
        }

        public bool Outcome(ModObjects.Activities.Action action, TestWeapon owner, TestPerson initiator, Dice diceRoll, int targetDefence)
        {
            var result = action.CreateOutcomeResult();

            var diceRollResult = diceRoll.Roll();
            if (diceRollResult >= targetDefence)
            {
                action
                    .ResetProp("damage")
                    .SetProp("damage", owner, x => x.Damage)
                    .SetProp("damage", initiator, x => x.StrengthBonus);
            }

            action.SetOutcomeState(initiator, nameof(Attacking), new Lifespan(0, 1));

            return true;
        }
    }
}
