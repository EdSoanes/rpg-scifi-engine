using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Actions;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class HumanShootsGunTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new RpgContainer("Room");
            location.AddToStore("Room", initiator);
            location.AddToStore("Room", recipient);
            location.AddToStore("Room", gun);

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
            Assert.That(gun.GetActions().Count(), Is.EqualTo(1));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0);
            Assert.That(fireInst, Is.Not.Null);

            fireInst.CostArgs["owner"] = gun;
            fireInst.CostArgs["initiator"] = initiator;

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            fireInst.ActArgs["owner"] = gun;
            fireInst.ActArgs["initiator"] = initiator;
            fireInst.ActArgs["targetDefense"] = 2;

            var actModSet = fireInst.Act();
            initiator.AddModSet(actModSet);
            graph.Time.TriggerEvent();

            var diceRoll = fireInst.ActResult;
            Assert.That(diceRoll.ToString(), Is.EqualTo("2d6 + 2"));

            fireInst.OutcomeArgs["owner"] = gun;
            fireInst.OutcomeArgs["initiator"] = initiator;
            fireInst.OutcomeArgs["diceRoll"] = 20;
            fireInst.OutcomeArgs["target"] = 15;

            var outcomeModSet = fireInst.Outcome();
            initiator.AddModSet(actModSet);
            graph.Time.TriggerEvent();

            var outcome = fireInst.Outcome;

            //Assert.That(fireGun, Is.Not.Null);
            //Assert.That(fireGun.DisabledWhen.Count(), Is.EqualTo(1));
            //Assert.That(fireGun.DisabledWhen, Does.Contain(nameof(TestGun.AmmoEmpty)));

            //var damage = gun.GetCommand(nameof(TestGun.InflictDamage));
            //Assert.That(damage, Is.Not.Null);
            //Assert.That(damage.EnabledWhen.Count(), Is.EqualTo(1));
            //Assert.That(damage.EnabledWhen, Does.Contain(nameof(TestGun.Shoot)));


            //Assert.That(gun.StateNames.Count(), Is.EqualTo(3));
            //Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.AmmoEmpty)));
            //Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.Shoot)));
            //Assert.That(gun.StateNames, Does.Contain(nameof(TestGun.InflictDamage)));
            //Assert.That(gun.ActiveStateNames.Count(), Is.EqualTo(0));

            //Assert.That(gun.HitBonus, Is.EqualTo(2));
            //Assert.That(gun.Damage.Dice.ToString(), Is.EqualTo("1d6"));
            //Assert.That(gun.Ammo.Max, Is.EqualTo(10));
            //Assert.That(gun.Ammo.Current, Is.EqualTo(10));
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureCost()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new RpgContainer("Room");
            location.AddToStore("Room", initiator);
            location.AddToStore("Room", recipient);
            location.AddToStore("Room", gun);

            var graph = new RpgGraph(location);
            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(5));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0)!;
            fireInst.CostArgs["owner"] = gun;
            fireInst.CostArgs["initiator"] = initiator;

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(4));
            //var fireGun = gun.GetAction(nameof(FireGunAction))!;
            //var args = fireGun.ArgSet(initiator);

            //Assert.That(args.ArgNames().Count(), Is.EqualTo(4));
            //Assert.That(args.ArgNames(), Does.Contain("modSet"));
            //Assert.That(args.ArgNames(), Does.Contain("initiator"));
            //Assert.That(args.ArgNames(), Does.Contain("targetDefense"));
            //Assert.That(args.ArgNames(), Does.Contain("targetRange"));

            //args
            //    .Set("targetDefense", recipient.Defense)
            //    .Set("targetRange", 10);

            ////Assert the modSets
            //var modSet = fireGun.Create(args)!;
            //var shootSubsets = modSet.SubSets(graph);

            //var target = shootSubsets.FirstOrDefault(x => x.TargetProp == modSet.TargetPropName);
            //Assert.That(target, Is.Not.Null);
            //Assert.That(target.IsResolved, Is.True);
            //Assert.That(target.Dice.Roll(), Is.EqualTo(5));

            //var diceRoll = shootSubsets.FirstOrDefault(x => x.TargetProp == modSet.DiceRollPropName);
            //Assert.That(diceRoll, Is.Not.Null);
            //Assert.That(diceRoll.IsResolved, Is.False);
            //Assert.That(diceRoll.Dice.ToString(), Is.EqualTo("1d20 + 4"));

            ////Actually shoot the gun...
            //fireGun.Apply(modSet);

            ////Assert the gun
            //Assert.That(gun.IsStateOn(nameof(FireGunAction)), Is.True);
            //Assert.That(gun.Ammo.Current, Is.EqualTo(9));

            ////Assert the initiator
            //Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(4));
            //Assert.That(initiator.MentalActionPoints.Current, Is.EqualTo(2));
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureAct()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new RpgContainer("Room");
            location.AddToStore("Room", initiator);
            location.AddToStore("Room", recipient);
            location.AddToStore("Room", gun);

            var graph = new RpgGraph(location);
            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(5));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0)!;
            fireInst.CostArgs["owner"] = gun;
            fireInst.CostArgs["initiator"] = initiator;

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            fireInst.ActArgs["owner"] = gun;
            fireInst.ActArgs["initiator"] = initiator;
            fireInst.ActArgs["targetDefense"] = 2;

            var actModSet = fireInst.Act();
            initiator.AddModSet(actModSet);
            graph.Time.TriggerEvent();

            Assert.That(graph.CalculatePropValue(initiator, $"{nameof(FireGunAction)}_ActResult_0")?.ToString(), Is.EqualTo("2d6 + 2"));
            Assert.That(fireInst.ActResult.ToString(), Is.EqualTo("2d6 + 2"));
        }

        [Test]
        public void HumanShootsGun_WoundRecipient_EnsureValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new RpgContainer("Room");
            location.AddToStore("Room", initiator);
            location.AddToStore("Room", recipient);
            location.AddToStore("Room", gun);

            var graph = new RpgGraph(location);
            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            ////Get the gun command and the args needed to execute it
            //var shootCmd = gun.GetCommand(nameof(TestGun.Shoot))!;
            //var shootArgs = shootCmd
            //    .ArgSet(initiator, recipient)
            //    .Set("targetDefense", recipient.Defense)
            //    .Set("targetRange", 10);

            //var shootModSet = shootCmd.Create(shootArgs);
            ////shootCmd.Apply(shootModSet);

            ////Simulate outcome of shooting the test gun
            //var target = shootCmd.GetTarget(initiator, shootModSet)!.Value;
            //var outcome = target + 1;

            ////Get the inflict damage command and set the args needed to execute it
            //var damageCmd = gun.GetCommand(shootCmd.OutcomeAction!);
            //var damageArgs = damageCmd!
            //    .ArgSet(initiator, recipient)
            //    .SetTarget(target)
            //    .SetOutcome(outcome);

            ////Find which mod subsets need resolving (dice rolls)
            //var damageModSet = damageCmd.Create(damageArgs)!;
            //var damageSet = damageModSet.SubSets(graph).Single(x => !x.IsResolved);

            ////TODO: How to we determine what the outcome mod lifecycle and behavior should be??
            //var damageRoll = damageSet.Resolve(new ExpireOnZeroMod(), -5);

            //Assert.That(damageSet.TargetId, Is.EqualTo(recipient.Id));

            ////Apply the resolved dice rolls to the recipient(s)
            //Assert.That(recipient.Health, Is.EqualTo(10));
            //recipient.AddMods(damageRoll);
            //graph.Time.TriggerEvent();

            //Assert.That(recipient.Health, Is.EqualTo(5));
        }
    }
}
