using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Actions;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
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

            var location = new Room();
            location.Contents.Add(initiator);
            location.Contents.Add(recipient);
            location.Contents.Add(gun);

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
            Assert.That(gun.GetAction(nameof(FireGunAction)), Is.Not.Null);

            graph.Time.SetTime(TimePoints.Encounter(1));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0);
            Assert.That(fireInst, Is.Not.Null);

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            fireInst.SetArgValue("targetDefence", 1);

            var actionModSet = fireInst.Act();
            initiator.AddModSets(actionModSet);
            graph.Time.TriggerEvent();

            var diceRoll = actionModSet.DiceRoll(graph);
            Assert.That(diceRoll.ToString(), Is.EqualTo("1d20 + 2"));

            fireInst.OutcomeArgs!
                .SetArg("diceRoll", 20)
                .SetArg("target", 15);

            var outcomeModSets = fireInst.Outcome();
            graph.AddModSets(outcomeModSets);
            graph.Time.TriggerEvent();

            Assert.That(gun.IsStateOn(nameof(GunFiring)), Is.True);

            var outcome = fireInst.Outcome;
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureCost()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new Room();
            location.Contents.Add(initiator);
            location.Contents.Add(recipient);
            location.Contents.Add(gun);

            var graph = new RpgGraph(location);
            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(5));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0)!;

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(4));
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureAct()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new Room();
            location.Contents.Add(initiator);
            location.Contents.Add(recipient);
            location.Contents.Add(gun);

            var graph = new RpgGraph(location);
            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            Assert.That(initiator.PhysicalActionPoints.Current, Is.EqualTo(5));

            var fireInst = gun.CreateActionInstance(initiator, nameof(FireGunAction), 0)!;

            var costModSet = fireInst.Cost();
            initiator.AddModSet(costModSet);
            graph.Time.TriggerEvent();

            fireInst.ActArgs!
                .SetArg("targetDefence", 1);

            var actionModSet = fireInst.Act();
            initiator.AddModSets(actionModSet);
            graph.Time.TriggerEvent();

            Assert.That(actionModSet.DiceRoll(graph).ToString(), Is.EqualTo("1d20 + 2"));
            Assert.That(actionModSet.Target(graph).ToString(), Is.EqualTo("11"));
        }

        [Test]
        public void HumanShootsGun_WoundRecipient_EnsureValues()
        {
            var initiator = new TestHuman();
            var recipient = new TestHuman();
            var gun = new TestGun();

            var location = new Room();
            location.Contents.Add(initiator);
            location.Contents.Add(recipient);
            location.Contents.Add(gun);

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
