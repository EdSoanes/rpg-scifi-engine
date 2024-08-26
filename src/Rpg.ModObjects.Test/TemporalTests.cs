using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class TemporalTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test] 
        public void CreateTemporal_Ok()
        {
            var temporal = new Temporal();
            Assert.That(temporal, Is.Not.Null);
            Assert.That(temporal.Current.Type, Is.EqualTo(PointInTimeType.BeforeTime));

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.TriggerEvent();

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.BeforeTime));
        }

        [Test]
        public void BeforeTime_To_TimePassing_Ok()
        {
            var temporal = new Temporal();

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.TimePassing);

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(PointInTimeType.TimePassing));
        }

        [Test]
        public void BeforeTime_To_Turn1_Ok()
        {
            var temporal = new Temporal();

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.Turn, 1);

            Assert.That(events.Count, Is.EqualTo(5));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(PointInTimeType.TimePassing));
            Assert.That(events[3].Type, Is.EqualTo(PointInTimeType.EncounterBegins));
            Assert.That(events[4].Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(events[4].Count, Is.EqualTo(1));
        }

        [Test]
        public void BeforeTime_To_EncounterEnds_Ok()
        {
            var temporal = new Temporal();

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.EncounterEnds);

            Assert.That(events.Count, Is.EqualTo(6));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(PointInTimeType.TimePassing));
            Assert.That(events[3].Type, Is.EqualTo(PointInTimeType.EncounterBegins));
            Assert.That(events[4].Type, Is.EqualTo(PointInTimeType.EncounterEnds));
            Assert.That(events[5].Type, Is.EqualTo(PointInTimeType.TimePassing));
        }

        [Test]
        public void TimePassing_To_OneHourPasses_Ok()
        {
            var temporal = new Temporal();

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.TimePassing);
            temporal.Transition(PointInTimeType.Hour);

            Assert.That(events.Count, Is.EqualTo(5));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(PointInTimeType.TimePassing));
            Assert.That(events[3].Type, Is.EqualTo(PointInTimeType.Hour));
            Assert.That(events[4].Type, Is.EqualTo(PointInTimeType.TimePassing));
        }

        [Test]
        public void Turn1_To_Turn2_Ok()
        {
            var temporal = new Temporal();

            temporal.Transition(PointInTimeType.Turn, 1);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.Turn, 2);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(events[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void Turn2_To_Turn1_Ok()
        {
            var temporal = new Temporal();

            temporal.Transition(PointInTimeType.Turn, 2);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.Turn, 1);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(events[0].Count, Is.EqualTo(1));
        }

        [Test]
        public void Turn2_To_Turn2_NoEvents()
        {
            var temporal = new Temporal();

            temporal.Transition(PointInTimeType.Turn, 2);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.Turn, 2);

            Assert.That(events.Count, Is.EqualTo(0));
        }

        [Test]
        public void Turn2_To_EncounterEnds_Ok()
        {
            var temporal = new Temporal();

            temporal.Transition(PointInTimeType.Turn, 2);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.EncounterEnds);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.EncounterEnds));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimePassing));
        }

        [Test]
        public void Turn2_To_EncounterEncounterBegins_Ok()
        {
            var temporal = new Temporal();

            temporal.Transition(PointInTimeType.Turn, 2);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Transition(PointInTimeType.EncounterBegins);

            Assert.That(events.Count, Is.EqualTo(4));
            Assert.That(events[0].Type, Is.EqualTo(PointInTimeType.EncounterEnds));
            Assert.That(events[1].Type, Is.EqualTo(PointInTimeType.TimePassing));
            Assert.That(events[2].Type, Is.EqualTo(PointInTimeType.EncounterBegins));
            Assert.That(events[3].Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(events[3].Count, Is.EqualTo(1));
        }

        [Test]
        public void TimePassing_To_BeforeTime_Fail()
        {
            var temporal = new Temporal();
            temporal.Transition(PointInTimeType.TimePassing);

            var events = new List<PointInTime>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            Assert.Throws<InvalidOperationException>(() => temporal.Transition(new PointInTime(PointInTimeType.BeforeTime)));
        }

        [Test]
        public void SpanOfTime_Encounter_TimePassing_Pending()
        {
            var span = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.GetExpiry(new PointInTime(PointInTimeType.TimePassing));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Pending));
        }

        public void SpanOfTime_Encounter_EncounterBegins_Active()
        {
            var span = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.GetExpiry(new PointInTime(PointInTimeType.EncounterBegins));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_Turn1_Active()
        {
            var span = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.GetExpiry(new PointInTime(PointInTimeType.EncounterBegins));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_EncounterEnds_Destroyed()
        {
            var span = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.GetExpiry(new PointInTime(PointInTimeType.EncounterEnds));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
        }


        [Test]
        public void SpanOfTime_TwoTurns_OnTurnThree_Expired()
        {
            var span = new SpanOfTime(new PointInTime(PointInTimeType.Turn, 1), new PointInTime(PointInTimeType.Turn, 3));
            var expiry = span.GetExpiry(new PointInTime(PointInTimeType.Turn, 3));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Expired));
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingEncounterBegins_False()
        {
            var span1 = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new SpanOfTime(PointInTimeType.TimePassing, PointInTimeType.EncounterBegins);

            Assert.That(span2.OverlapsWith(span1), Is.False);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingTurn1_False()
        {
            var span1 = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new SpanOfTime(PointInTimeType.TimePassing, 1);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_Turn_True()
        {
            var span1 = new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new SpanOfTime(1, 2);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Turn1_OverlapsWith_Turn2_False()
        {
            var span1 = new SpanOfTime(1, 1);
            var span2 = new SpanOfTime(2, 1);

            Assert.That(span2.OverlapsWith(span1), Is.False);
        }

        [Test]
        public void PointInTime_Turn1_LessThan_Turn2()
        {
            var p1 = new PointInTime(1);
            var p2 = new PointInTime(2);

            Assert.That(p1 < p2, Is.True);
        }

        [Test]
        public void PointInTime_Turn2_GreaterThan_Turn1()
        {
            var p1 = new PointInTime(1);
            var p2 = new PointInTime(2);

            Assert.That(p2 > p1, Is.True);
        }

        [Test]
        public void PointInTime_Turn1_Equals_Turn1()
        {
            var p1 = new PointInTime(1);
            var p2 = new PointInTime(1);

            Assert.That(p1 == p2, Is.True);
        }
    }
}
