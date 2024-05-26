using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class HumanShootsGunTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureValues()
        {
            var initiator = new TestHuman();
            var target = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(target);
            location.Add(gun);

            var graph = new ModGraph(location);

            //Initiator
            Assert.That(initiator.Strength.Score, Is.EqualTo(14));
            Assert.That(initiator.Strength.Bonus, Is.EqualTo(2));

            Assert.That(initiator.Intelligence.Score, Is.EqualTo(10));
            Assert.That(initiator.Intelligence.Bonus, Is.EqualTo(0));

            Assert.That(initiator.Dexterity.Score, Is.EqualTo(17));
            Assert.That(initiator.Dexterity.Bonus, Is.EqualTo(3));

            Assert.That(initiator.MeleeAttack, Is.EqualTo(4));
            Assert.That(initiator.MeleeDamage.Dice.ToString(), Is.EqualTo("1d6 + 2"));
            Assert.That(initiator.MissileAttack, Is.EqualTo(2));
            Assert.That(initiator.Defense, Is.EqualTo(5));
            Assert.That(initiator.Health, Is.EqualTo(10));

            Assert.That(initiator.PhysicalActionPoints.Max, Is.EqualTo(5));
            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(5));
            Assert.That(initiator.MentalActionPoints.Max, Is.EqualTo(3));
            Assert.That(initiator.MentalActionPoints.Current, Is.EqualTo(3));

            //Gun
            Assert.That(gun.GetCommands().Count(), Is.EqualTo(2));
            Assert.That(gun.GetCommands().SingleOrDefault(x => x.CommandName == nameof(TestGun.Shoot)), Is.Not.Null);
            Assert.That(gun.GetCommands().Single(x => x.CommandName == nameof(TestGun.Shoot)).OutcomeCommandName, Is.EqualTo(nameof(TestGun.InflictDamage)));
            Assert.That(gun.GetCommands().SingleOrDefault(x => x.CommandName == nameof(TestGun.InflictDamage)), Is.Not.Null);

            Assert.That(gun.StateNames.Count(), Is.EqualTo(3));
            Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.AmmoEmpty)));
            Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.Shoot)));
            Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.InflictDamage)));
            Assert.That(gun.ActiveStateNames.Count(), Is.EqualTo(0));

            Assert.That(gun.HitBonus, Is.EqualTo(2));
            Assert.That(gun.Damage.Dice.ToString(), Is.EqualTo("1d6"));
            Assert.That(gun.Ammo.Max, Is.EqualTo(10));
            Assert.That(gun.Ammo.Current, Is.EqualTo(10));
        }

        [Test]
        public void HumanShootsGun_ShootGunCmd_EnsureCmdValues()
        {
            var initiator = new TestHuman();
            var target = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(target);
            location.Add(gun);

            var graph = new ModGraph(location);
            graph.NewEncounter();

            var shootCmd = gun.GetCommand(nameof(TestGun.Shoot));
            Assert.That(shootCmd, Is.Not.Null);

            var args = shootCmd.ExecutionArgSet();

            Assert.That(shootCmd.DisabledOnStates.Count(), Is.EqualTo(1));
            Assert.That(shootCmd.DisabledOnStates, Does.Contain(nameof(TestGun.AmmoEmpty)));

            Assert.That(args.Count(), Is.EqualTo(2));
            Assert.That(args.Keys, Does.Contain("targetDefense"));
            Assert.That(args.Keys, Does.Contain("targetRange"));

            args["targetDefense"] = target.Defense;
            args["targetRange"] = 10;
            
            var modSet = shootCmd.Execute(initiator, args);

            Assert.That(gun.ActiveStateNames, Does.Contain(nameof(TestGun.Shoot)));
            Assert.That(gun.Ammo.Current, Is.EqualTo(9));
            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(4));
            Assert.That(initiator.MentalActionPoints.Current, Is.EqualTo(2));

            var targetRoll = initiator.GetPropValue(modSet!.TargetProp);
            Assert.That(targetRoll, Is.Not.Null);
            Assert.That(targetRoll!.Value.Roll(), Is.EqualTo((DiceCalculations.Range(10) + target.Defense).Roll()));

            var diceRoll = initiator.GetPropValue(modSet!.DiceRollProp);
            Assert.That(diceRoll, Is.Not.Null);
            Assert.That(diceRoll!.Value.ToString(), Is.EqualTo("1d20 + 4"));
        }

        [Test]
        public void HumanShootsGun_ShootGunCmd_InflictDamageCmd_EnsureOutcomeValues()
        {
            var initiator = new TestHuman();
            var target = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(target);
            location.Add(gun);

            var graph = new ModGraph(location);
            graph.NewEncounter();

            var shootCmd = gun.GetCommand(nameof(TestGun.Shoot))!;
            var shootArgs = shootCmd.ExecutionArgSet();
            shootArgs["targetDefense"] = target.Defense;
            shootArgs["targetRange"] = 10;
            var shootModSet = shootCmd.Execute(initiator, shootArgs);

            var damageCmd = gun.GetCommand(shootCmd.OutcomeCommandName!);
            var damageArgs = damageCmd!.ExecutionArgSet();

            var targetRoll = shootCmd.GetTargetRoll(initiator, shootModSet);
            var outcome = targetRoll + 1;

            var damageModSet = damageCmd.Execute(initiator, targetRoll, null, outcome, damageArgs);

            var damageRoll = damageCmd.GetDiceRoll(initiator, damageModSet);
            Assert.That(damageRoll, Is.Not.Null);
            Assert.That(damageRoll.ToString(), Is.EqualTo(gun.Damage.Dice.ToString()));
        }
    }
}
