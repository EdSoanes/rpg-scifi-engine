using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModStateTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
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
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddMod(new PermanentMod("mymod"), x => x.Melee, 6);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(10));
            Assert.That(entity.Health, Is.EqualTo(20));

            var mods = graph.GetMods(entity, "Melee", mod => mod.Name == "mymod");
            graph.RemoveMods(mods);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
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
