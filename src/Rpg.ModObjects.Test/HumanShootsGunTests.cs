using Rpg.ModObjects.Modifiers;
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
            RpgGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(recipient);
            location.Add(gun);

            var graph = new RpgGraph(location);

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

            var shoot = gun.GetCommand(nameof(TestGun.Shoot));
            Assert.That(shoot, Is.Not.Null);
            Assert.That(shoot.DisabledOnStates.Count(), Is.EqualTo(1));
            Assert.That(shoot.DisabledOnStates, Does.Contain(nameof(TestGun.AmmoEmpty)));

            var damage = gun.GetCommand(nameof(TestGun.InflictDamage));
            Assert.That(damage, Is.Not.Null);
            Assert.That(damage.EnabledOnStates.Count(), Is.EqualTo(1));
            Assert.That(damage.EnabledOnStates, Does.Contain(nameof(TestGun.Shoot)));


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
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(recipient);
            location.Add(gun);

            var graph = new RpgGraph(location);
            graph.NewEncounter();

            var shootCmd = gun.GetCommand(nameof(TestGun.Shoot))!;
            var args = shootCmd.ArgSet(initiator);

            Assert.That(args.ArgNames().Count(), Is.EqualTo(4));
            Assert.That(args.ArgNames(), Does.Contain("modSet"));
            Assert.That(args.ArgNames(), Does.Contain("initiator"));
            Assert.That(args.ArgNames(), Does.Contain("targetDefense"));
            Assert.That(args.ArgNames(), Does.Contain("targetRange"));

            args
                .Set("targetDefense", recipient.Defense)
                .Set("targetRange", 10);
            
            //Assert the modSets
            var modSet = shootCmd.Create(args)!;
            var shootSubsets = modSet.SubSets(graph);

            var target = shootSubsets.FirstOrDefault(x => x.Name == modSet.TargetProp);
            Assert.That(target, Is.Not.Null);
            Assert.That(target.IsResolved, Is.True);
            Assert.That(target.Dice.Roll(), Is.EqualTo(11));

            var diceRoll = shootSubsets.FirstOrDefault(x => x.Name == modSet.DiceRollProp);
            Assert.That(diceRoll, Is.Not.Null);
            Assert.That(diceRoll.IsResolved, Is.False);
            Assert.That(diceRoll.Dice.ToString(), Is.EqualTo("1d20 + 4"));

            //Assert the gun
            Assert.That(gun.IsStateActive(nameof(TestGun.Shoot)), Is.True);
            Assert.That(gun.Ammo.Current, Is.EqualTo(9));

            //Assert the initiator
            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(4));
            Assert.That(initiator.MentalActionPoints.Current, Is.EqualTo(2));
        }

        [Test]
        public void HumanShootsGun_ShootGunCmd_InflictDamageCmd_EnsureOutcomeValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(recipient);
            location.Add(gun);

            var graph = new RpgGraph(location);
            graph.NewEncounter();

            //Get the gun command and the args needed to execute it
            var shootCmd = gun.GetCommand(nameof(TestGun.Shoot))!;
            var shootArgs = shootCmd
                .ArgSet(initiator, recipient)
                .Set("targetDefense", recipient.Defense)
                .Set("targetRange", 10);

            var shootModSet = shootCmd.Create(shootArgs);
            var shootSubSets = shootModSet?.SubSets(graph).Where(x => !x.IsResolved);

            shootCmd.Apply(shootModSet);

            var target = shootCmd.GetTarget(initiator, shootModSet)!.Value;
            var outcome = target + 1;

            var damageCmd = gun.GetCommand(shootCmd.OutcomeCommandName!);
            var damageArgs = damageCmd!
                .ArgSet(initiator, recipient)
                .SetTarget(target)
                .SetOutcome(outcome);

            var damageModSet = damageCmd.Create(damageArgs);
            var damageRoll = damageModSet?.SubSets(graph).FirstOrDefault(x => !x.IsResolved);

            Assert.That(damageRoll, Is.Not.Null);
            Assert.That(damageRoll.Dice.ToString(), Is.EqualTo(damageRoll.Dice.ToString()));
        }

        [Test]
        public void HumanShootsGun_WoundRecipient_EnsureValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(recipient);
            location.Add(gun);

            var graph = new RpgGraph(location);
            graph.NewEncounter();

            //Get the gun command and the args needed to execute it
            var shootCmd = gun.GetCommand(nameof(TestGun.Shoot))!;
            var shootArgs = shootCmd
                .ArgSet(initiator, recipient)
                .Set("targetDefense", recipient.Defense)
                .Set("targetRange", 10);

            var shootModSet = shootCmd.Create(shootArgs);
            //shootCmd.Apply(shootModSet);

            //Simulate outcome of shooting the test gun
            var target = shootCmd.GetTarget(initiator, shootModSet)!.Value;
            var outcome = target + 1;

            //Get the inflict damage command and set the args needed to execute it
            var damageCmd = gun.GetCommand(shootCmd.OutcomeCommandName!);
            var damageArgs = damageCmd!
                .ArgSet(initiator, recipient)
                .SetTarget(target)
                .SetOutcome(outcome);

            //Find which mod subsets need resolving (dice rolls)
            var damageModSet = damageCmd.Create(damageArgs)!;
            var damageSet = damageModSet.SubSets(graph).Single(x => !x.IsResolved);
            var damageRoll = damageSet.Resolve(new ExpireOnZero(), -5);

            Assert.That(damageSet.TargetId, Is.EqualTo(recipient.Id));

            //Apply the resolved dice rolls to the recipient(s)
            Assert.That(recipient.Health, Is.EqualTo(10));
            recipient.AddMod(damageRoll);
            recipient.TriggerUpdate();

            Assert.That(recipient.Health, Is.EqualTo(5));
        }
    }
}
