using Rpg.Experimental.Activities;
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
            var graph = new RpgGraph(obj);

            var actions = graph.GetObjectActions(obj.Id);
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
            var graph = new RpgGraph(obj);
            var testAction = graph.GetObjectAction(obj.Id, nameof(TestAction));

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.IsPerformable, Is.False);
            Assert.That(testAction.CanPerformArgsComplete, Is.True);
        }

        [Test]
        public void TestAction_CanPerform_True()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var testAction = graph.GetObjectAction(obj.Id, nameof(TestAction));

            graph.Add(obj, x => x.Strength, 1);
            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.IsPerformable, Is.True);
            Assert.That(testAction.CanPerformArgsComplete, Is.True);
        }

        [Test]
        public void TestAction_CreateActivity_EnsureValues()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var activity = graph.CreateActivity(obj.Id, obj.Id, nameof(TestAction));

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.ActivityActions.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAction_CreateActivity_Perform()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var activity = graph.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.ActivityActions.First() as RpgActivityAction;

            Assert.That(activityAction, Is.Not.Null);

            Assert.That(activityAction.CostMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.CostMethod.Execute(graph), Is.True);

            RpgArg.SetValue(graph, activityAction.PerformMethod.Args, "value", 1);
            Assert.That(activityAction.PerformMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.PerformMethod.Execute(graph), Is.True);

            RpgArg.SetValue(graph, activityAction.OutcomeMethod.Args, "value", 1);
            Assert.That(activityAction.OutcomeMethod.Args.IsComplete(), Is.True);
            Assert.That(activityAction.OutcomeMethod.Execute(graph), Is.True);

            Assert.That(activityAction.AllStepsComplete, Is.True);
            Assert.That(activityAction.IsComplete, Is.False);
            
            activityAction.Complete();
            graph.Time.Refresh();

            Assert.That(activityAction.IsComplete, Is.True);
            Assert.That(activityAction.Result.IsApplied, Is.True);
            Assert.That(activityAction.Result.Mods.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAction_CreateActivity_Perform_AssertActivityArgs()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var activity = graph.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.ActivityActions.First() as RpgActivityAction;

            Assert.That(activity, Is.Not.Null);
            Assert.That(activityAction, Is.Not.Null);

            Assert.That(activityAction.Args.Count(), Is.EqualTo(5));
            Assert.That(activity.Args.Count(), Is.EqualTo(5));
            Assert.That(activityAction.Args.Find("value")?.Value, Is.Null);
            Assert.That(activity.Args.Find("value")?.Value, Is.Null);

            Assert.That(activityAction.Cost(graph), Is.True);

            Assert.That(activityAction.Perform(graph, ("value", 1)), Is.True);
            Assert.That(activityAction.Args.Find("value")?.Value, Is.EqualTo(1));
            Assert.That(activity.Args.Find("value")?.Value, Is.EqualTo(1));

            Assert.That(activityAction.Outcome(graph, ("value", 2)), Is.True);
            Assert.That(activityAction.Args.Find("value")?.Value, Is.EqualTo(2));
            Assert.That(activity.Args.Find("value")?.Value, Is.EqualTo(2));
        }
    }
}