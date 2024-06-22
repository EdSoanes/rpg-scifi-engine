using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Actions
{
    public class FireGunAction : ModObjects.Actions.Action<TestGun>
    {
        public FireGunAction(TestGun owner)
            : base(owner) { }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            => owner.GetState(nameof(AmmoEmptyState))?.Off() ?? true;

        public ModSet OnCost(TestGun owner, TestHuman initiator)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ModSet OnAct(TestGun owner, TestHuman initiator, int targetDefence)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), initiator, $"{nameof(FireGunAction)}.{nameof(OnAct)}", "1d20")
                .AddMod(new TurnMod(), initiator, $"{nameof(FireGunAction)}.{nameof(OnAct)}", x => x.MissileAttack)
                .AddMod(new TurnMod(), initiator, $"{nameof(FireGunAction)}.{nameof(OnAct)}", -targetDefence);
        }

        public ModSet[] OnOutcome(TestGun owner, TestHuman initiator, int target, int diceRoll)
        {
            var damage = new ModSet(owner)
                .AddMod(new TurnMod(), owner, $"{nameof(FireGunAction)}.{nameof(OnOutcome)}", x => x.Damage.Dice)
                .AddMod(new TurnMod(), owner, $"{nameof(FireGunAction)}.{nameof(OnOutcome)}", initiator, x => x.Dexterity.Bonus);

            var ammo = new Modification<TestGun>(owner)
                .AddMod(new PermanentMod(), owner, x => x.Ammo.Current, -1);

            var res = new List<ModSet>
            {
                damage,
                ammo,
                owner.GetState(nameof(FireGunAction))!
            };

            return res.ToArray();
        }
    }
}
