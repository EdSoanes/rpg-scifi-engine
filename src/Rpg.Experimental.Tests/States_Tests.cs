using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{
    public class States_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void Intelligence4_TestStateActive_InitiativeIs1()
        {
            var obj = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState));
            Assert.That(testState, Is.Not.Null);
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));

            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);

            characterSheet.Add(obj, x => x.Intelligence, 1);
            characterSheet.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Intelligence, Is.EqualTo(4));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));
        }

        [Test]
        public void Intelligence3_Manual_TestStateActive_InitiativeIs1()
        {
            var obj = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState));
            Assert.That(testState, Is.Not.Null);
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));

            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);

            testState.UserEnabled();
            characterSheet.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));

            testState.UserEnabledReset();
            characterSheet.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);
        }

        [Test]
        public void Intelligence3_Activate_TestStateActive_InitiativeIs1()
        {
            var obj = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState));
            Assert.That(testState, Is.Not.Null);
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));

            characterSheet.Time.BeginEncounter();

            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);

            var activationId = characterSheet.ActivateState(obj.Id, nameof(TestState), 1);
            characterSheet.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));

            characterSheet.Time.ToTurn(2);

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);
        }
    }
}