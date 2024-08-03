using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;

namespace Rpg.ModObjects.Tests
{
    public class StateEntity : RpgEntity
    {
        public int Value { get; protected set; } = 4;
        public int BuffedValue { get; protected set; }
    }

    public class Buff : State<StateEntity>
    {
        public Buff(StateEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(StateEntity owner)
            => owner.Value >= 10;

        protected override void OnFillStateSet(ModSet modSet, StateEntity owner)
            => modSet.Add(owner, x => x.BuffedValue, 10);
    }

    public class ModStateTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void IncreaseMeleeTo10_ActivateBuffState_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.IsStateOn(nameof(BuffState)), Is.False);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));

            var mod = entity.AddMod(new PermanentMod(), x => x.Melee, 6);
            graph.Time.TriggerEvent();

            Assert.That(entity.IsStateOn(nameof(BuffState)), Is.True);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(10));
            Assert.That(entity.Health, Is.EqualTo(20));
        }

        [Test]
        public void ActivateBuffState_DecreaseMeleeTo4_VerifyValues()
        {
            var entity = new StateEntity();
            var graph = new RpgGraph(entity);

            entity.AddMod(new PermanentMod("mymod"), x => x.Value, 6);
            graph.Time.TriggerEvent();

            Assert.That(entity.Value, Is.EqualTo(10));
            Assert.That(entity.BuffedValue, Is.EqualTo(10));

            var mods = graph.GetActiveMods(entity, nameof(StateEntity.Value), mod => mod.Name == "mymod");
            graph.RemoveMods(mods);
            graph.Time.TriggerEvent();

            Assert.That(entity.Value, Is.EqualTo(4));
            Assert.That(entity.BuffedValue, Is.EqualTo(0));
        }

        [Test]
        public void DecreaseMeleeTo0_ActivateNerfState_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));

            entity.AddMod(new PermanentMod(), x => x.Melee, -4);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(0));
            Assert.That(entity.Health, Is.EqualTo(0));
        }

        [Test]
        public void ManuallyActivateBuffState_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));

            entity.SetStateOn(nameof(BuffState));
            graph.Time.TriggerEvent();

            Assert.That(entity.IsStateOn(nameof(BuffState)), Is.True);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(20));

            entity.SetStateOff(nameof(BuffState));
            graph.Time.TriggerEvent();

            Assert.That(entity.IsStateOn(nameof(BuffState)), Is.False);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
        }
    }
}
