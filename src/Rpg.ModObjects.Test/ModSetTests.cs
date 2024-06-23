using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;
using System.Reflection;

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
            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));

            entity.AddModSet("name", modSet =>
            {
                modSet
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Melee, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Health, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(14));
        }

        [Test]
        public void AddModSet_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Melee, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Health, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSet(entity, "test")!;
            modSet.Lifecycle.SetExpired(graph.Time.Current);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));
        }

        [Test]
        public void AddModSetWithPermanentMod_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));
            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Health, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Damage.ArmorPenetration, 1)
                    .AddMod(new PermanentMod(), entity, x => x.Melee, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSet(entity, "test")!;
            modSet.Lifecycle.SetExpired(graph.Time.Current);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(12));
        }

        [Test]
        public void AddModSet_RemoveModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));

            entity.AddModSet("test", modSet =>
            {
                modSet
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Melee, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Health, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSet(entity, "test")!;
            graph.RemoveModSet(entity, modSet.Id);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));
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
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Melee, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Health, 1)
                    .AddMod(new SyncedMod(modSet.Id), entity, x => x.Damage.ArmorPenetration, 1);
            });

            graph.Time.TriggerEvent();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(1));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(14));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));

            graph.Time.SetTime(TimePoints.EndOfEncounter);
            graph.Time.TriggerEvent();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(0));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(11));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
        }
    }
}
