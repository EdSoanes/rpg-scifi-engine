using Rpg.Experimental.Graph;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.States;
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

            var graph = new RpgGraph(obj);
            var objData = graph.GetObjectData(obj.Id);

            var testState = graph.GetObjectState(obj.Id, nameof(TestState));
            Assert.That(testState, Is.Not.Null);
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));

            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.Initiative, Is.Null);

            graph.Add(obj, x => x.Intelligence, 1);
            graph.Time.Refresh();

            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Intelligence, Is.EqualTo(4));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));
        }
    }
}