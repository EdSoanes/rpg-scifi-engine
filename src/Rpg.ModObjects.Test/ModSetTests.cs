using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModSetTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void AddModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(9));

            entity.AddModSet("name", modSet =>
            {
                modSet
                    .AddMod(entity, x => x.Melee, 1)
                    .AddMod(entity, x => x.Health, 1)
                    .AddMod(entity, x => x.Damage.ArmorPenetration, 1);
            });

            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));
        }

        [Test]
        public void AddModSet_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(entity, x => x.Melee, 1)
                    .AddMod(entity, x => x.Health, 1)
                    .AddMod(entity, x => x.Damage.ArmorPenetration, 1);
            });

            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));

            var modSet = entity.GetModSet("test")!;
            modSet.SetExpired();
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));
        }

        [Test]
        public void AddModSetWithPermanentMod_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(entity, x => x.Health, 1)
                    .AddMod(entity, x => x.Damage.ArmorPenetration, 1)
                    .Add(PermanentMod.Create(entity, x => x.Melee, 1));
            });

            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));

            var modSet = entity.GetModSet("test")!;
            modSet.SetExpired();
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));
        }

        [Test]
        public void AddModSet_RemoveModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(9));

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(entity, x => x.Melee, 1)
                    .AddMod(entity, x => x.Health, 1)
                    .AddMod(entity, x => x.Damage.ArmorPenetration, 1);
            });

            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));

            var modSet = entity.GetModSet("test")!;
            entity.RemoveModSet(modSet.Id);
            entity.TriggerUpdate();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(9));
        }

        [Test]
        public void AddEncounterModSet_EndEncounter_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            graph.NewEncounter();

            entity.AddModSet("name", ModDuration.OnEndEncounter(), modSet =>
            {
                modSet
                    .AddMod(entity, x => x.Melee, 1)
                    .AddMod(entity, x => x.Health, 1)
                    .AddMod(entity, x => x.Damage.ArmorPenetration, 1);
            });

            entity.TriggerUpdate();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(1));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));

            graph.EndEncounter();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(0));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(9));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
        }
    }
}
