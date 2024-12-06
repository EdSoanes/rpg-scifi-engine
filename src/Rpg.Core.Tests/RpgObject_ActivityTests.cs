using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.Core.Tests
{
    public class RpgObject_ActivityTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestWeapon_Attack_EnsureInitialized()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            var activity = person.InitiateAction(weapon, nameof(Attack));
            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Active));

            var action = activity.CurrentAction();
            Assert.That(action, Is.Not.Null);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));
        }

        [Test]
        public void TestWeapon_Attack_CanPerform()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);
            graph.Time.Transition(PointInTimeType.EncounterBegins);

            Assert.That(person.CanInitiateAction(weapon, nameof(Attack)), Is.True);
            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;

            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));
        }

        [Test]
        public void TestWeapon_Attack_CannotPerform_NotHoldingWeapon()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Wearing.Add(weapon);

            var graph = new RpgGraph(person);
            graph.Time.Transition(PointInTimeType.EncounterBegins);

            Assert.That(person.CanInitiateAction(weapon, nameof(Attack)), Is.False);
            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;

            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));
        }

        [Test]
        public void TestWeapon_Attack_CannotPerform_WeaponDamaged()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);
            weapon.SetStateOn(nameof(WeaponDamaged));

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            Assert.That(weapon.IsStateOn(nameof(WeaponDamaged)));

            Assert.That(person.CanInitiateAction(weapon, nameof(Attack)), Is.False);
            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;

            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));
        }

        [Test]
        public void TestWeapon_Attack_Perform()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            Assert.That(person.CanInitiateAction(weapon, nameof(Attack)), Is.True);
            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;

            var performArgs = activity.PerformArgs();
            Assert.That(performArgs.IsComplete, Is.False);
            Assert.That(performArgs.Count(), Is.EqualTo(4));
            Assert.That(performArgs.Has("targetDefence"), Is.True);
            Assert.That(performArgs.Find("targetDefence")!.Value, Is.Null);

            performArgs.Set("targetDefence", 15);

            var performResult = activity.Perform(performArgs);
            Assert.That(performResult, Is.True);
            Assert.That(action.Value("diceRoll").ToString(), Is.EqualTo("1d20 + 2"));
            Assert.That(action.Value("targetDefence").ToString(), Is.EqualTo("15"));
            Assert.That(action.Status, Is.EqualTo(ActionStatus.Started));
        }

        [Test]
        public void TestWeapon_Attack_Outcome()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;

            activity.Cost(activity.CostArgs());

            var performArgs = activity.PerformArgs();
            performArgs.Set("targetDefence", 15);

            activity.Perform(performArgs);

            var outcomeArgs = activity.OutcomeArgs();
            Assert.That(outcomeArgs.IsComplete(), Is.True);
            Assert.That(outcomeArgs.Count(), Is.EqualTo(5));
            Assert.That(outcomeArgs.Val("diceRoll"), Is.EqualTo(new Dice("1d20 + 2")));
            Assert.That(outcomeArgs.Val("targetDefence"), Is.EqualTo(15));

            outcomeArgs.Set("diceRoll", 20);
            outcomeArgs.Set("targetDefence", 15);

            var outcomeResult = activity.Outcome(outcomeArgs);
            Assert.That(outcomeResult, Is.True);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.CanComplete));
            Assert.That(action.Value("damage").ToString(), Is.EqualTo("1d6 + 1"));
            Assert.That(action.OutcomeModSet.Mods.Count(), Is.EqualTo(0));

            Assert.That(action.OutcomeStates.Count(), Is.EqualTo(1));
            Assert.That(action.OutcomeStates[0].State, Is.EqualTo(nameof(Attacking)));
            Assert.That(action.OutcomeStates[0].OwnerId, Is.EqualTo(person.Id));
            Assert.That(person.IsStateOn(nameof(Attacking)), Is.False);

            var nextActions = activity.Complete();
            Assert.That(action.Status, Is.EqualTo(ActionStatus.Completed));
            Assert.That(nextActions.Length, Is.EqualTo(0));
            Assert.That(person.IsStateOn(nameof(Attacking)), Is.True);
        }
    }
}
