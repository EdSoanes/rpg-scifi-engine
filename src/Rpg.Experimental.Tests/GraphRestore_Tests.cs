using Rpg.Experimental.Json;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// A graph state must survive a round trip through json and still be fully functional (the sessionless
    /// server does this on every request)
    /// </summary>
    public class GraphRestore_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static RpgCharacterSheet RoundTrip(RpgCharacterSheet characterSheet)
        {
            var json = RpgJson.SerializeGraphState(characterSheet.GetState());
            var state = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json);

            return new RpgCharacterSheet(state, characterSheet.GetSystem());
        }

        [Test]
        public void Restore_EnsureValues()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Add(obj, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));

            var characterSheet2 = RoundTrip(characterSheet);
            var obj2 = characterSheet2.GetObject(obj.Id) as TestObject;

            Assert.That(obj2, Is.Not.Null);
            Assert.That(characterSheet2.Context.Id, Is.EqualTo(obj.Id));
            Assert.That(characterSheet2.Actor.Id, Is.EqualTo(obj.Id));
            Assert.That(characterSheet2.GetObjectCount(), Is.EqualTo(characterSheet.GetObjectCount()));
            Assert.That(characterSheet2.Time.Now, Is.EqualTo(characterSheet.Time.Now));

            Assert.That(obj2.Strength, Is.EqualTo(14));
            Assert.That(obj2.StrengthBonus, Is.EqualTo(2));
            Assert.That(obj2.Child, Is.Not.Null);
            Assert.That(obj2.Child.Id, Is.EqualTo(obj.Child.Id));

            //Mods can still be added to the restored graph
            characterSheet2.Add(obj2, x => x.Strength, 2);
            characterSheet2.Time.Refresh();

            Assert.That(obj2.Strength, Is.EqualTo(16));

            //Meta data is relinked
            var strength = characterSheet2.GetProperties(obj.Id).FirstOrDefault(x => x.Prop == nameof(TestObject.Strength));
            Assert.That(strength, Is.Not.Null);
            Assert.That(strength.DisplayName, Is.EqualTo("Str"));
        }

        [Test]
        public void Restore_ModSet_KeepsMods()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));

            var characterSheet2 = RoundTrip(characterSheet);
            var obj2 = (characterSheet2.GetObject(obj.Id) as TestObject)!;
            var modSet2 = characterSheet2.GetLifecycleObject(modSet.Id) as RpgModSet;

            Assert.That(modSet2, Is.Not.Null);
            Assert.That(modSet2.Mods.Count, Is.EqualTo(1));
            Assert.That(obj2.Strength, Is.EqualTo(11));

            modSet2.Unapply();
            characterSheet2.Time.Refresh();

            Assert.That(obj2.Strength, Is.EqualTo(10));

            modSet2.Apply();
            characterSheet2.Time.Refresh();

            Assert.That(obj2.Strength, Is.EqualTo(11));

            modSet2.Expire(characterSheet2);
            characterSheet2.Time.Refresh();

            Assert.That(obj2.Strength, Is.EqualTo(10));
            Assert.That(characterSheet2.ObjectExists(modSet.Id), Is.False);
        }

        [Test]
        public void Restore_State_KeepsMods()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState))!;
            testState.UserEnabled();
            characterSheet.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));

            var characterSheet2 = RoundTrip(characterSheet);
            var obj2 = (characterSheet2.GetObject(obj.Id) as TestObject)!;
            var testState2 = characterSheet2.GetObjectState(obj.Id, nameof(TestState));

            Assert.That(testState2, Is.Not.Null);
            Assert.That(testState2.Mods.Count, Is.EqualTo(1));
            Assert.That(testState2.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj2.Initiative, Is.EqualTo(new Dice(1)));

            testState2.UserDisabled();
            characterSheet2.Time.Refresh();

            Assert.That(testState2.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(obj2.Initiative, Is.Null);
        }

        [Test]
        public void Restore_Actions_StillPerformable()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Add(obj, x => x.Strength, 1);
            characterSheet.Time.Refresh();

            var characterSheet2 = RoundTrip(characterSheet);
            var testAction2 = characterSheet2.GetObjectAction(obj.Id, nameof(TestAction));

            Assert.That(testAction2, Is.Not.Null);
            Assert.That(testAction2.Args.Length, Is.EqualTo(5));
            Assert.That(testAction2.IsPerformable, Is.True);
        }

        [Test]
        public void Restore_MidActivity_ContinueToComplete()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.BeginEncounter();

            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.CurrentActivityAction!;

            Assert.That(activityAction.Cost(characterSheet), Is.True);
            Assert.That(activityAction.Perform(characterSheet, ("value", 1)), Is.True);

            //Round trip between the Perform and Outcome steps
            var characterSheet2 = RoundTrip(characterSheet);
            var activity2 = characterSheet2.GetObject(activity.Id) as RpgActivity;

            Assert.That(activity2, Is.Not.Null);
            Assert.That(activity2.ActivityActions.Count, Is.EqualTo(1));
            Assert.That(activity2.Args.Count(), Is.EqualTo(5));
            Assert.That(activity2.Args.Find("value")?.Value, Is.EqualTo(1));

            var activityAction2 = activity2.CurrentActivityAction;

            Assert.That(activityAction2, Is.Not.Null);
            Assert.That(activityAction2.Id, Is.EqualTo(activityAction.Id));
            Assert.That(activityAction2.GetAction(), Is.Not.Null);
            Assert.That(activityAction2.Result, Is.Not.Null);
            Assert.That(activityAction2.CostMethod.IsDone, Is.True);
            Assert.That(activityAction2.PerformMethod.IsDone, Is.True);
            Assert.That(activityAction2.OutcomeMethod.IsDone, Is.False);
            Assert.That(characterSheet2.GetPropertyValue<Dice?>(activityAction2.Id, "value"), Is.EqualTo(new Dice(1)));

            Assert.That(activityAction2.Outcome(characterSheet2, ("value", 2)), Is.True);
            Assert.That(activityAction2.AllStepsComplete, Is.True);
            Assert.That(activityAction2.Result.Mods.Count, Is.EqualTo(1));

            //Round trip again between Outcome and Complete
            var characterSheet3 = RoundTrip(characterSheet2);
            var activityAction3 = (characterSheet3.GetObject(activity.Id) as RpgActivity)?.CurrentActivityAction;

            Assert.That(activityAction3, Is.Not.Null);
            Assert.That(activityAction3.AllStepsComplete, Is.True);
            Assert.That(activityAction3.OutcomeActions.Count, Is.EqualTo(1));
            Assert.That(activityAction3.Result.Mods.Count, Is.EqualTo(1));
            Assert.That(activityAction3.Result.IsApplied, Is.False);

            var testState3 = characterSheet3.GetObjectState(obj.Id, nameof(TestState))!;
            Assert.That(testState3.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));

            var outcomeActions = activityAction3.Complete(characterSheet3);
            characterSheet3.Time.Refresh();

            Assert.That(outcomeActions.Length, Is.EqualTo(1));
            Assert.That(activityAction3.IsComplete, Is.True);
            Assert.That(activityAction3.Result.IsApplied, Is.True);

            //The outcome of the test action activates the test state for one turn
            var obj3 = (characterSheet3.GetObject(obj.Id) as TestObject)!;
            Assert.That(testState3.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj3.Initiative, Is.EqualTo(new Dice(1)));

            characterSheet3.Time.ToTurn(2);

            Assert.That(testState3.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(obj3.Initiative, Is.Null);
        }
    }
}
