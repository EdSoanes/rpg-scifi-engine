using Newtonsoft.Json;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
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

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => owner.GetState(nameof(AmmoEmptyState))?.Off() ?? true;

        public ModSet OnCost(TestGun owner, TestHuman initiator)
        {
            return new ModSet(new TimeLifecycle(TimePoints.Encounter(1)))
                .AddMod(new TurnMod(), initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ModSet OnAct(int actionNo, TestGun owner, TestHuman initiator, int targetDefence)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));

            ActResultMod(actionNo, modSet, initiator, "Base", "2d6");
            ActResultMod(actionNo, modSet, initiator, x => x.MissileAttack);
            ActResultMod(actionNo, modSet, initiator, "TargetDefence", -targetDefence);

            return modSet;
        }

        public ModSet[] OnOutcome(int actionNo, TestGun owner, TestHuman initiator, int target, int diceRoll)
        {
            var outcome = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));
            OutcomeMod(actionNo, outcome, initiator, owner, x => x.Damage.Dice);
            OutcomeMod(actionNo, outcome, initiator, x => x.Dexterity.Bonus);

            var ammo = new Modification<TestGun>(owner)
                .AddMod(new PermanentMod(), owner, x => x.Ammo.Current, -1);

            var firing = owner.CreateStateInstance(nameof(FireGunAction), new TimeLifecycle(TimePoints.Encounter(1)));
            var res = new List<ModSet>
            {
                outcome,
                ammo,
                firing
            };

            return res.ToArray();
        }
    }
}
