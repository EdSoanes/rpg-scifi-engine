using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Props.Attributes;
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
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
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

            var thresholdMod = mods.FirstOrDefault(x => x.Behavior is Threshold);
            Assert.That(thresholdMod, Is.Not.Null);

            var threshold = thresholdMod.Behavior as Threshold;
            Assert.That(threshold, Is.Not.Null);
            Assert.That(threshold.Min, Is.EqualTo(1));
        }

        [Test]
        public void ApplyThresholdMod_EnsureMinValue()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            var thresholdMod = graph
                .GetActiveMods(entity, nameof(ThresholdEntity.MinValue))
                .First(x => x.Behavior is Threshold);

            var threshold = thresholdMod.Behavior as Threshold;
            Assert.That(threshold, Is.Not.Null);
            Assert.That(threshold.Min, Is.EqualTo(1));
        }

        [Test]
        public void SetValueBelowThreshold_ValueEqualsMin()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MinValue, Is.EqualTo(2));
            entity.AddMod(new PermanentMod(), x => x.MinValue, -200);
            graph.Time.TriggerEvent();

            Assert.That(entity.MinValue, Is.EqualTo(1));
        }

        [Test]
        public void SetMaxValueAboveThreshold_ValueEqualsMax()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MaxValue, Is.EqualTo(2));
            entity.AddMod(new PermanentMod(), x => x.MaxValue, 200);
            graph.Time.TriggerEvent();

            Assert.That(entity.MaxValue, Is.EqualTo(10));
        }

        [Test]
        public void SetMaxValueBelowThreshold_ValueLessThanMax()
        {
            var entity = new ThresholdEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.MaxValue, Is.EqualTo(2));
            entity.AddMod(new PermanentMod(), x => x.MaxValue, 7);
            graph.Time.TriggerEvent();

            Assert.That(entity.MaxValue, Is.EqualTo(9));
        }
    }
}
