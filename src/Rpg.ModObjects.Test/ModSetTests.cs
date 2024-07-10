using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class ModSetTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void AddModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            entity.AddModSet("name", modSet =>
            {
                modSet
                    .Add(entity, x => x.Melee, 1)
                    .Add(entity, x => x.Health, 1)
                    .Add(entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(15));
        }

        [Test]
        public void AddModSet_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .Add(entity, x => x.Melee, 1)
                    .Add(entity, x => x.Health, 1)
                    .Add(entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.Lifecycle.SetExpired(graph.Time.Current);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
        }

        [Test]
        public void AddModSetWithPermanentMod_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(12));
            entity.AddModSet("test", modSet =>
            {
                modSet
                    .Add(entity, x => x.Health, 1)
                    .Add(entity, x => x.Damage.ArmorPenetration, 1)
                    .Add(new PermanentMod(), entity, x => x.Melee, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(15));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.Lifecycle.SetExpired(graph.Time.Current);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(13));
        }

        [Test]
        public void AddModSet_RemoveModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .Add(entity, x => x.Melee, 1)
                    .Add(entity, x => x.Health, 1)
                    .Add(entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            graph.RemoveModSet(entity, modSet.Id);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
        }

        [Test]
        public void AddEncounterModSet_EndEncounter_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddModSet("name", graph.Time.Create("encounter").Lifecycle, modSet =>
            {
                modSet
                    .Add(entity, x => x.Melee, 1)
                    .Add(entity, x => x.Health, 1)
                    .Add(entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(1));
            var mods = graph.GetActiveMods();
            Assert.That(mods.Count(), Is.EqualTo(14));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));

            graph.Time.SetTime(TimePoints.EndOfEncounter);
            graph.Time.TriggerEvent();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(0));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
        }
    }
}
