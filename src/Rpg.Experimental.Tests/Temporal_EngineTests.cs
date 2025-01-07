using Rpg.Experimental.Reflection;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{
    public class Temporal_EngineTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(this.GetType().Assembly);
        }

        [Test] 
        public void CreateTemporal_Ok()
        {
            var temporal = new Temporal();
            Assert.That(temporal, Is.Not.Null);
            Assert.That(temporal.Now.Type, Is.EqualTo(TimePointType.BeforeTime));

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.Refresh();

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.BeforeTime));
        }

        [Test]
        public void BeforeTime_To_TimePassing_Ok()
        {
            var temporal = new Temporal();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.TimePasses();

            Assert.That(events.Count, Is.EqualTo(4));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(TimePointType.TimePasses));
            Assert.That(events[3].Type, Is.EqualTo(TimePointType.Waiting));
        }

        [Test]
        public void BeforeTime_To_Turn1_Ok()
        {
            var temporal = new Temporal();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.BeginEncounter();

            Assert.That(events.Count, Is.EqualTo(4));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(TimePointType.EncounterBegins));
            Assert.That(events[3].Type, Is.EqualTo(TimePointType.Turn));
            Assert.That(events[3].Count, Is.EqualTo(1));
        }

        [Test]
        public void BeforeTime_To_EncounterEnds_Ok()
        {
            var temporal = new Temporal();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.EndEncounter();

            Assert.That(events.Count, Is.EqualTo(5));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.BeforeTime));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.TimeBegins));
            Assert.That(events[2].Type, Is.EqualTo(TimePointType.EncounterBegins));
            Assert.That(events[3].Type, Is.EqualTo(TimePointType.EncounterEnds));
            Assert.That(events[4].Type, Is.EqualTo(TimePointType.Waiting));
        }

        [Test]
        public void TimePassing_To_TimePasses2_Ok()
        {
            var temporal = new Temporal();
            temporal.TimePasses();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.TimePasses(2);


            Assert.That(events.Count, Is.EqualTo(4));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.TimePasses));
            Assert.That(events[0].Count, Is.EqualTo(0));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.TimePasses));
            Assert.That(events[1].Count, Is.EqualTo(1));
            Assert.That(events[2].Type, Is.EqualTo(TimePointType.TimePasses));
            Assert.That(events[2].Count, Is.EqualTo(2));
            Assert.That(events[3].Type, Is.EqualTo(TimePointType.Waiting));
        }

        [Test]
        public void Turn1_To_Turn2_Ok()
        {
            var temporal = new Temporal();

            temporal.BeginEncounter();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.ToTurn(2);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.Turn));
            Assert.That(events[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void Turn2_To_Turn1_Ok()
        {
            var temporal = new Temporal();

            temporal.ToTurn(2);

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.ToTurn(1);

            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.Turn));
            Assert.That(events[0].Count, Is.EqualTo(1));
        }

        [Test]
        public void Turn2_To_Turn2_NoEvents()
        {
            var temporal = new Temporal();

            temporal.ToTurn(2);

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.ToTurn(2);

            Assert.That(events.Count, Is.EqualTo(0));
        }

        [Test]
        public void Turn2_To_EncounterEnds_Ok()
        {
            var temporal = new Temporal();

            temporal.ToTurn(2);

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.EndEncounter();

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.EncounterEnds));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.Waiting));
        }

        [Test]
        public void Turn2_To_EncounterEncounterBegins_Ok()
        {
            var temporal = new Temporal();

            temporal.ToTurn(2);

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            temporal.BeginEncounter();

            Assert.That(events.Count, Is.EqualTo(4));
            Assert.That(events[0].Type, Is.EqualTo(TimePointType.EncounterEnds));
            Assert.That(events[1].Type, Is.EqualTo(TimePointType.Waiting));
            Assert.That(events[2].Type, Is.EqualTo(TimePointType.EncounterBegins));
            Assert.That(events[3].Type, Is.EqualTo(TimePointType.Turn));
            Assert.That(events[3].Count, Is.EqualTo(1));
        }

        [Test]
        public void TimePassing_To_BeforeTime_Fail()
        {
            var temporal = new Temporal();

            temporal.TimePasses();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (obj, e) => events.Add(e.Time);

            Assert.Throws<InvalidOperationException>(() => temporal.BeginTime());
        }
    }
}
