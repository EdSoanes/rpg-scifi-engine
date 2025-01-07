using Rpg.Experimental.Reflection;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{
    public class Temporal_TimePointTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(this.GetType().Assembly);
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
