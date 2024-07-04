using Newtonsoft.Json;
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

        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
            => owner.GetState(nameof(AmmoEmptyState))?.Off() ?? true;

        public ModSet OnCost(TestGun owner, TestHuman initiator)
        {
            return new ModSet(initiator, new TurnLifecycle())
                .Add(initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ModSet[] OnAct(int actionNo, TestGun owner, TestHuman initiator, int targetDefence)
        {
            var modSet = new ModSet(initiator, new TurnLifecycle());

            ActResult(actionNo, modSet, initiator, "Base", "d20");
            ActResult(actionNo, modSet, initiator, x => x.MissileAttack);
            ActResult(actionNo, modSet, initiator, "TargetDefence", -targetDefence);

            return [modSet];
        }

        public ModSet[] OnOutcome(int actionNo, TestGun owner, TestHuman initiator, int target, int diceRoll)
        {
            var outcome = new ModSet(initiator, new TurnLifecycle());
            OutcomeMod(actionNo, outcome, initiator, owner, x => x.Damage.Dice);
            OutcomeMod(actionNo, outcome, initiator, x => x.Dexterity.Bonus);

            var ammo = new ModSet(owner, new TurnLifecycle())
                .Add(new PermanentMod(), owner, x => x.Ammo.Current, -1);

            var firing = owner.CreateStateInstance(nameof(GunFiring), new TurnLifecycle());
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
