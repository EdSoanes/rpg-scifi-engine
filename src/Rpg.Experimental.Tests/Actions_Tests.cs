using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    public class Actions_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void Assert_Initial_Values()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            var actions = characterSheet.GetObjectActions(obj.Id);
            Assert.That(actions.Count(), Is.EqualTo(1));

            var testAction = actions.Single();
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction, Is.TypeOf<TestAction>());
            Assert.That(testAction.CanPerformMethod, Is.Not.Null);
            Assert.That(testAction.CostMethod, Is.Not.Null);
            Assert.That(testAction.PerformMethod, Is.Not.Null);
            Assert.That(testAction.OutcomeMethod, Is.Not.Null);

            Assert.That(testAction.Args.Length, Is.EqualTo(5));
        }

        [Test]
        public void TestAction_CanPerform_False()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var testAction = characterSheet.GetObjectAction(obj.Id, nameof(TestAction));

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.IsPerformable, Is.False);
            Assert.That(testAction.CanPerformArgsComplete, Is.True);
        }

        [Test]
        public void TestAction_CanPerform_True()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var testAction = characterSheet.GetObjectAction(obj.Id, nameof(TestAction));

            characterSheet.Add(obj, x => x.Strength, 1);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.IsPerformable, Is.True);
            Assert.That(testAction.CanPerformArgsComplete, Is.True);
        }

        [Test]
        public void TestAction_CreateActivity_EnsureValues()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.ActivityActions.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAction_CreateActivity_Perform()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.ActivityActions.First() as RpgActivityAction;

            Assert.That(activityAction, Is.Not.Null);

            Assert.That(activityAction.CostMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.CostMethod.Execute(characterSheet), Is.True);

            RpgArg.SetValue(characterSheet, activityAction.PerformMethod.Args, "value", 1);
            Assert.That(activityAction.PerformMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.PerformMethod.Execute(characterSheet), Is.True);

            RpgArg.SetValue(characterSheet, activityAction.OutcomeMethod.Args, "value", 1);
            Assert.That(activityAction.OutcomeMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.OutcomeMethod.Execute(characterSheet), Is.True);

            Assert.That(activityAction.AllStepsComplete, Is.True);
            Assert.That(activityAction.IsComplete, Is.False);
            
            activityAction.Complete(characterSheet);
            characterSheet.Time.Refresh();

            Assert.That(activityAction.IsComplete, Is.True);
            Assert.That(activityAction.Result.IsApplied, Is.True);
            Assert.That(activityAction.Result.Mods.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAction_CreateActivity_Perform_AssertActivityArgs()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.ActivityActions.First() as RpgActivityAction;

            Assert.That(activity, Is.Not.Null);
            Assert.That(activityAction, Is.Not.Null);

            //Action args are virtual properties on the activity action, merged by name into the activity args
            Assert.That(activityAction.CostMethod.Args.Count(), Is.EqualTo(2));
            Assert.That(activityAction.PerformMethod.Args.Count(), Is.EqualTo(2));
            Assert.That(activityAction.OutcomeMethod.Args.Count(), Is.EqualTo(3));
            Assert.That(activity.Args.Count(), Is.EqualTo(5));
            Assert.That(characterSheet.GetPropertyValue<Dice?>(activityAction.Id, "value"), Is.Null);
            Assert.That(activity.Args.Find("value")?.Value, Is.Null);

            Assert.That(activityAction.Cost(characterSheet), Is.True);

            Assert.That(activityAction.Perform(characterSheet, ("value", 1)), Is.True);
            Assert.That(characterSheet.GetPropertyValue<Dice?>(activityAction.Id, "value"), Is.EqualTo(new Dice(1)));
            Assert.That(activityAction.OutcomeMethod.Args.Find("value")?.Value, Is.EqualTo(1));
            Assert.That(activity.Args.Find("value")?.Value, Is.EqualTo(1));

            Assert.That(activityAction.Outcome(characterSheet, ("value", 2)), Is.True);
            Assert.That(characterSheet.GetPropertyValue<Dice?>(activityAction.Id, "value"), Is.EqualTo(new Dice(2)));
            Assert.That(activity.Args.Find("value")?.Value, Is.EqualTo(2));
        }

        [Test]
        public void TestAction_Steps_ViaActivityAction_Complete()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.CurrentActivityAction!;

            Assert.That(activityAction.Outcome(characterSheet), Is.False);

            Assert.That(activityAction.Cost(characterSheet), Is.True);
            Assert.That(activityAction.Perform(characterSheet, ("value", 1)), Is.True);
            Assert.That(activityAction.Outcome(characterSheet), Is.True);
            Assert.That(activityAction.AllStepsComplete, Is.True);

            var outcomeActions = activityAction.Complete(characterSheet);
            characterSheet.Time.Refresh();

            Assert.That(activityAction.IsComplete, Is.True);
            Assert.That(activityAction.Result.IsApplied, Is.True);
            Assert.That(outcomeActions.Length, Is.EqualTo(1));
            Assert.That(outcomeActions[0].ActionOwnerId, Is.EqualTo(obj.Id));
            Assert.That(outcomeActions[0].ActionName, Is.EqualTo(nameof(TestAction)));
            Assert.That(outcomeActions[0].Optional, Is.True);
        }

        [Test]
        public void TestAction_OutcomeAction_ContinuesActivity_ValuesFlowDown()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var first = activity.CurrentActivityAction!;

            Assert.That(first.Cost(characterSheet), Is.True);
            Assert.That(first.Perform(characterSheet, ("value", 3)), Is.True);
            Assert.That(first.Outcome(characterSheet), Is.True);

            var outcomeActions = first.Complete(characterSheet);
            characterSheet.Time.Refresh();

            Assert.That(outcomeActions.Length, Is.EqualTo(1));

            var activity2 = characterSheet.CreateActivity(obj.Id, outcomeActions[0]);
            Assert.That(activity2.Id, Is.EqualTo(activity.Id));
            Assert.That(activity.ActivityActions.Count, Is.EqualTo(2));

            var second = activity.CurrentActivityAction!;
            Assert.That(second.Id, Is.Not.EqualTo(first.Id));
            Assert.That(second.ActivityActionNo, Is.EqualTo(2));
            Assert.That(second.IsComplete, Is.False);

            //The value set on the first action is carried into the second
            Assert.That(characterSheet.GetPropertyValue<Dice?>(second.Id, "value"), Is.EqualTo(new Dice(3)));
            Assert.That(second.PerformMethod.Args.IsComplete(), Is.True);

            //...and can be replaced without affecting the first
            Assert.That(second.Cost(characterSheet), Is.True);
            Assert.That(second.Perform(characterSheet, ("value", 5)), Is.True);
            Assert.That(characterSheet.GetPropertyValue<Dice?>(second.Id, "value"), Is.EqualTo(new Dice(5)));
            Assert.That(characterSheet.GetPropertyValue<Dice?>(first.Id, "value"), Is.EqualTo(new Dice(3)));
            Assert.That(activity.Args.Find("value")?.Value, Is.EqualTo(5));
        }

        [Test]
        public void TestAction_Reset_ClearsOutcome()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.CurrentActivityAction!;

            Assert.That(activityAction.Cost(characterSheet), Is.True);
            Assert.That(activityAction.Perform(characterSheet, ("value", 1)), Is.True);
            Assert.That(activityAction.Outcome(characterSheet), Is.True);
            Assert.That(activityAction.OutcomeActions.Count, Is.EqualTo(1));

            activityAction.Reset(characterSheet, ActionMethodNames.Outcome);

            Assert.That(activityAction.OutcomeMethod.IsDone, Is.False);
            Assert.That(activityAction.PerformMethod.IsDone, Is.True);
            Assert.That(activityAction.OutcomeActions.Count, Is.EqualTo(0));
            Assert.That(activityAction.Complete(characterSheet).Length, Is.EqualTo(0));
            Assert.That(activityAction.IsComplete, Is.False);
        }
    }
}