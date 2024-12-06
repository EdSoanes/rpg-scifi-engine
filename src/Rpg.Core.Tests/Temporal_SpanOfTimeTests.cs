using Rpg.Core.Tests.Models;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Core.Tests
{
    public class Temporal_SpanOfTimeTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void SpanOfTime_WaitingToTimePasses_EncounterBegins_Expired()
        {
            var span = new Lifespan(PointInTimeType.Waiting, PointInTimeType.TimePasses);
            var expiry = span.UpdateExpiry(PointInTimeType.EncounterBegins);

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
        }

        [Test]
        public void SpanOfTime_Encounter_TimePassing_Pending()
        {
            var span = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.UpdateExpiry(PointInTimeType.Waiting);

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Pending));
        }

        public void SpanOfTime_Encounter_EncounterBegins_Active()
        {
            var span = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.UpdateExpiry(PointInTimeType.EncounterBegins);

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_Turn1_Active()
        {
            var span = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.UpdateExpiry(PointInTimeType.EncounterBegins);

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Active));
        }

        [Test]
        public void SpanOfTime_Encounter_EncounterEnds_Destroyed()
        {
            var span = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var expiry = span.UpdateExpiry(PointInTimeType.EncounterEnds);

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
        }


        [Test]
        public void SpanOfTime_TwoTurns_OnTurnThree_Expired()
        {
            var span = new Lifespan(new PointInTime(PointInTimeType.Turn, 1), new PointInTime(PointInTimeType.Turn, 3));
            var expiry = span.UpdateExpiry(new PointInTime(PointInTimeType.Turn, 3));

            Assert.That(expiry, Is.EqualTo(LifecycleExpiry.Expired));
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingEncounterBegins_False()
        {
            var span1 = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new Lifespan(PointInTimeType.Waiting, PointInTimeType.EncounterBegins);

            Assert.That(span2.OverlapsWith(span1), Is.False);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_TimePassingTurn1_False()
        {
            var span1 = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new Lifespan(PointInTimeType.Waiting, 1);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Encounter_OverlapsWith_Turn_True()
        {
            var span1 = new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds);
            var span2 = new Lifespan(1, 2);

            Assert.That(span2.OverlapsWith(span1), Is.True);
        }

        [Test]
        public void SpanOfTime_Turn1_OverlapsWith_Turn2_False()
        {
            var span1 = new Lifespan(1, 1);
            var span2 = new Lifespan(2, 1);

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
