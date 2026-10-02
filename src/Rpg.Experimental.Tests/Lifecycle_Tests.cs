using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    public class Lifecycle_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void DifferentObjects_SameLifespan_AreNotEqual()
        {
            var obj1 = new TestObject("One");
            var obj2 = new TestObject("Two");
            var list = new List<RpgObject> { obj1 };

            Assert.That(obj1.HasSameLifespan(obj2), Is.True);
            Assert.That(obj1.Equals(obj2), Is.False);
            Assert.That(obj1 == obj2, Is.False);
            Assert.That(list.Contains(obj1), Is.True);
            Assert.That(list.Contains(obj2), Is.False);
        }

        [Test]
        public void Mod_StartsInOneTurn_ActiveOnNextTurnOnly()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.ToTurn(3);

            characterSheet.Add(new Temporal(1, 1), obj, x => x.Strength, 2);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(10));

            characterSheet.Time.ToTurn(4);
            Assert.That(obj.Strength, Is.EqualTo(12));

            characterSheet.Time.ToTurn(5);
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void Mod_RelativeLifespan_AddedOutsideTurnTracking_StartsTurnTracking()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);

            characterSheet.Add(new Temporal(1, 1), obj, x => x.Strength, 2);
            characterSheet.Time.Refresh();

            //The mod starts next turn, so the clock starts now
            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));
            Assert.That(obj.Strength, Is.EqualTo(10));

            characterSheet.Time.ToTurn(2);
            Assert.That(obj.Strength, Is.EqualTo(12));

            characterSheet.Time.ToTurn(3);
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void ModSet_ModWithOwnLifespan_KeepsIt()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.BeginEncounter();

            //The set lasts 3 turns, one of its mods only 1 turn
            var modSet = new RpgModSet().Lifespan(3);
            modSet.Add(new Temporal(1), obj, x => x.Strength, 2);
            modSet.Add(obj, x => x.Intelligence, 1);

            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));
            Assert.That(obj.Intelligence, Is.EqualTo(4));

            characterSheet.Time.ToTurn(2);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.Intelligence, Is.EqualTo(4));

            characterSheet.Time.ToTurn(4);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.Intelligence, Is.EqualTo(3));
        }

        [Test]
        public void ModSet_ModWithOwnLifespan_StillAppliedByTheSet()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.BeginEncounter();

            var modSet = new RpgModSet().Lifespan(3);
            modSet.Add(new Temporal(2), obj, x => x.Strength, 2);
            modSet.Unapply();

            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(10));

            modSet.Apply();
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));
        }

        [Test]
        public void State_OnAndOff_OwnerActiveStatesFollow()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState))!;

            Assert.That(testState.IsOn, Is.False);
            Assert.That(obj.IsStateOn(nameof(TestState)), Is.False);
            Assert.That(obj.ActiveStates, Is.Empty);

            characterSheet.Add(obj, x => x.Intelligence, 1);
            characterSheet.Time.Refresh();

            Assert.That(testState.IsOn, Is.True);
            Assert.That(obj.IsStateOn(nameof(TestState)), Is.True);
            Assert.That(obj.ActiveStates, Is.EqualTo(new[] { nameof(TestState) }));

            testState.UserDisabled();
            characterSheet.Time.Refresh();

            Assert.That(testState.IsOn, Is.False);
            Assert.That(obj.IsStateOn(nameof(TestState)), Is.False);
        }

        [Test]
        public void Move_ChildKeepsItsOwnDerivedValues()
        {
            var parent = new TestObject("Parent");
            var child = new TestObject("Child");
            parent.Child = child;

            var characterSheet = new RpgCharacterSheet(parent);
            characterSheet.Add(child, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(child.StrengthBonus, Is.EqualTo(2));

            characterSheet.Move(parent.Id, nameof(TestObject.Children), child.Id);
            characterSheet.Time.Refresh();

            Assert.That(parent.Child, Is.Null);
            Assert.That(parent.Children.Contains(child), Is.True);

            //The child's props derived from its own props are untouched by the move
            Assert.That(child.Strength, Is.EqualTo(14));
            Assert.That(child.StrengthBonus, Is.EqualTo(2));
            Assert.That(child.Damage.ToString(), Is.EqualTo("1d6 + 15"));

            characterSheet.Add(child, x => x.Strength, 2);
            characterSheet.Time.Refresh();

            Assert.That(child.StrengthBonus, Is.EqualTo(3));
        }

        [Test]
        public void Move_OtherObjectsModsSourcedFromChild_Expire()
        {
            var parent = new TestObject("Parent");
            var child = new TestObject("Child");
            parent.Child = child;

            var characterSheet = new RpgCharacterSheet(parent);
            characterSheet.Add(new Standard(), parent, x => x.Strength, child, x => x.StrengthBonus);
            characterSheet.Add(child, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(parent.Strength, Is.EqualTo(12));

            characterSheet.Move(parent.Id, nameof(TestObject.Children), child.Id);
            characterSheet.Time.Refresh();

            Assert.That(parent.Strength, Is.EqualTo(10));
            Assert.That(child.StrengthBonus, Is.EqualTo(2));
        }

        [Test]
        public void Action_RepeatedStep_NotRunAgain()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var activity = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction));
            var activityAction = activity.CurrentActivityAction!;

            Assert.That(activityAction.Cost(characterSheet), Is.True);
            Assert.That(activityAction.Perform(characterSheet, ("value", 1)), Is.True);

            //Repeating a completed step is not an error and does not run it again
            Assert.That(activityAction.Perform(characterSheet), Is.True);
            Assert.That(activityAction.Outcome(characterSheet), Is.True);
            Assert.That(activityAction.Outcome(characterSheet), Is.True);
            Assert.That(activityAction.Result.Mods.Count, Is.EqualTo(1));
        }
    }
}
