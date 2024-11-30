using Rpg.Core.Tests.Models;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Core.Tests
{
    public class Temporal_PointInTimeTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
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
