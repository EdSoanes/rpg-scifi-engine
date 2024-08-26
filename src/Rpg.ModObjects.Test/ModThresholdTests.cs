using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModThresholdTests
    {
        public class ThresholdEntity : RpgEntity
        {
            [Threshold(Min = 1)]
            public int MinValue { get; protected set; } = 2;

            [Threshold(Max = 10)]
            public int MaxValue { get; protected set; } = 2;

            [Threshold(Min = 1, Max = 10)]
            public int BoundedValue { get; protected set; } = 2;
        }

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void ApplyThresholdMod_EnsureSetup()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MinValue, Is.EqualTo(2));
            var mods = graph.GetActiveMods(entity, nameof(ThresholdEntity.MinValue));

            Assert.That(mods.Count(), Is.EqualTo(2));
            Assert.That(mods.All(x => x.IsBaseInitMod), Is.True);

            var thresholdMod = mods.FirstOrDefault(x => x is Threshold);
            Assert.That(thresholdMod, Is.Not.Null);

            var threshold = thresholdMod as Threshold;
            Assert.That(threshold, Is.Not.Null);
            var behavior = threshold.Behavior as Behaviors.Threshold;
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior.Min, Is.EqualTo(1));
        }

        [Test]
        public void ApplyThresholdMod_EnsureMinValue()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            var threshold = graph
                .GetActiveMods(entity, nameof(ThresholdEntity.MinValue))
                .FirstOrDefault(x => x is Threshold) as Threshold;

            Assert.That(threshold, Is.Not.Null);

            var behavior = threshold.Behavior as Behaviors.Threshold;
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior.Min, Is.EqualTo(1));
        }

        [Test]
        public void SetValueBelowThreshold_ValueEqualsMin()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MinValue, Is.EqualTo(2));
            entity.AddMod(new Permanent(), x => x.MinValue, -200);
            graph.Time.TriggerEvent();

            Assert.That(entity.MinValue, Is.EqualTo(1));
        }

        [Test]
        public void SetMaxValueAboveThreshold_ValueEqualsMax()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MaxValue, Is.EqualTo(2));
            entity.AddMod(new Permanent(), x => x.MaxValue, 200);
            graph.Time.TriggerEvent();

            Assert.That(entity.MaxValue, Is.EqualTo(10));
        }

        [Test]
        public void SetMaxValueBelowThreshold_ValueLessThanMax()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MaxValue, Is.EqualTo(2));
            entity.AddMod(new Permanent(), x => x.MaxValue, 7);
            graph.Time.TriggerEvent();

            Assert.That(entity.MaxValue, Is.EqualTo(9));
        }
    }
}
