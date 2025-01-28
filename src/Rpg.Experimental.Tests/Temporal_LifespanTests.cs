using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{
    public class Temporal_LifespanTests
    {
        RpgGraph _graph;

        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(this.GetType().Assembly);
            _graph = new RpgGraph(new TestObject());
        }

        [Test]
        public void SpanOfTime_WaitingToTimePasses_EncounterBegins_Destroyed()
        {
            var span = new RpgLifecycleObject(TimePointType.Waiting, TimePointType.TimePasses);
            _graph.Time.BeginEncounter();
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
        }

        [Test]
        public void SpanOfTime_Encounter_TimePassing_Pending()
        {
            var span = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            _graph.Time.Refresh();
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Pending));
        }

        public void SpanOfTime_Encounter_EncounterBegins_Active()
        {
            var span = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            _graph.Time.BeginEncounter();
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_Turn1_Active()
        {
            var span = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            _graph.Time.BeginEncounter();
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_EncounterEnds_Pending()
        {
            var span = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            _graph.Time.EndEncounter();
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Pending));
        }


        [Test]
        public void SpanOfTime_TwoTurns_OnTurnThree_Expired()
        {
            var span = new RpgLifecycleObject(new TimePoint(TimePointType.Turn, 1), new TimePoint(TimePointType.Turn, 3));
            _graph.Time.ToTurn(3);
            span.OnTimeEvent(_graph);

            Assert.That(span.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingEncounterBegins_False()
        {
            var span1 = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            var span2 = new RpgLifecycleObject(TimePointType.Waiting, TimePointType.EncounterBegins);

            Assert.That(span2.OverlapsWith(span1), Is.False);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingTurn1_False()
        {
            var span1 = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            var span2 = new RpgLifecycleObject(TimePointType.Waiting, 1);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_Turn_True()
        {
            var span1 = new RpgLifecycleObject(TimePointType.EncounterBegins, TimePointType.EncounterEnds);
            var span2 = new RpgLifecycleObject(1, 2);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Turn1_OverlapsWith_Turn2_False()
        {
            var span1 = new RpgLifecycleObject(1, 1);
            var span2 = new RpgLifecycleObject(2, 1);

            Assert.That(span2.OverlapsWith(span1), Is.False);
        }

        [Test]
        public void TimePoint_Turn1_LessThan_Turn2()
        {
            var p1 = new TimePoint(1);
            var p2 = new TimePoint(2);

            Assert.That(p1 < p2, Is.True);
        }

        [Test]
        public void TimePoint_Turn2_GreaterThan_Turn1()
        {
            var p1 = new TimePoint(1);
            var p2 = new TimePoint(2);

            Assert.That(p2 > p1, Is.True);
        }

        [Test]
        public void TimePoint_Turn1_Equals_Turn1()
        {
            var p1 = new TimePoint(1);
            var p2 = new TimePoint(1);

            Assert.That(p1 == p2, Is.True);
        }
    }
}
