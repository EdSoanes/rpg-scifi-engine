using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.Core.Tests
{
    public class RpgObject_Mods_BaseModTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestPerson_AddPermanentMeleeAttackMod_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Permanent(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(4));
        }

        [Test]
        public void TestPerson_AddOverrideStrengthMod_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Override(), x => x.Strength, 10);
            graph.Time.TriggerEvent();

            Assert.That(person.Strength, Is.EqualTo(10));
            Assert.That(person.StrengthBonus, Is.EqualTo(0));
            Assert.That(person.MeleeAttack, Is.EqualTo(1));
        }

        [Test]
        public void TestEntity_CreateDamageMod_CreateRepairMod_IsRepaired()
        {
            //var entity = new ModdableEntity();
            //var graph = new RpgGraph(entity);

            //Assert.That(entity.Health, Is.EqualTo(10));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            //entity.AddMod(new ExpireOnZero(), x => x.Health, -10);
            //graph.Time.TriggerEvent();

            //Assert.That(entity.Health, Is.EqualTo(0));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(12));

            //entity.AddMod(new ExpireOnZero(), x => x.Health, 10);
            //graph.Time.TriggerEvent();

            //Assert.That(entity.Health, Is.EqualTo(10));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
        }
    }
}
