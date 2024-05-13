using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModStateTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void IncreaseMeleeTo10_ActivateBuffState_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));

            var mod = entity.AddPermanentMod(x => x.Melee, 6);
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(10));
            Assert.That(entity.Health, Is.EqualTo(20));
        }

        [Test]
        public void ActivateBuffState_DecreaseMeleeTo4_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            var mod = entity.AddPermanentMod(x => x.Melee, 6);
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(10));
            Assert.That(entity.Health, Is.EqualTo(20));

            entity.RemoveMods(mod);
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
        }

        [Test]
        public void DecreaseMeleeTo0_ActivateNerfState_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));

            var mod = entity.AddPermanentMod(x => x.Melee, -4);
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(0));
            Assert.That(entity.Health, Is.EqualTo(0));
        }
    }
}
