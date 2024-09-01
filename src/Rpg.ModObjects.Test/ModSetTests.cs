using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Mods.ModSets;
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
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void AddModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));

            var activeMods = graph.GetActiveMods();
            Assert.That(activeMods.Count(), Is.EqualTo(11));

            entity.AddModSet(new ModSet(entity.Id, "name")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1));

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));
        }

        [Test]
        public void AddModSet_ExpireModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddModSet(new ModSet(entity.Id, "test")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1));

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.SetExpired();
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

            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
            entity.AddModSet(new ModSet(entity.Id, "test")
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1)
                .Add(new Permanent(), entity, x => x.Melee, 1));

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.SetExpired();
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(12));
        }

        [Test]
        public void AddModSet_UnapplyAndApplyModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            entity.AddModSet(new ModSet(entity.Id, "test")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1));

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.Unapply();
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            modSet.Apply();
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));
        }

        [Test]
        public void AddModSet_DisableAndEnableModSet_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            entity.AddModSet(new ModSet(entity.Id, "test")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1));

            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));

            var modSet = graph.GetModSets(entity, (x) => x.Name == "test").First();
            modSet.UserDisabled();
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            modSet.UserEnabled();
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(14));
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

            entity.AddModSet(new ModSet(entity.Id, "test")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1));

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

            graph.Time.Transition(PointInTimeType.Turn);

            var modSet = new TimedModSet(entity.Id, "name", new SpanOfTime(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds))
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Health, 1)
                .Add(entity, x => x.Damage.ArmorPenetration, 1);

            entity.AddModSet(modSet);

            graph.Time.TriggerEvent();

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(1));
            var mods = graph.GetActiveMods();
            Assert.That(mods.Count(), Is.EqualTo(14));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(5));
            Assert.That(entity.Health, Is.EqualTo(11));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(11));

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(graph.GetModSets().Count(), Is.EqualTo(0));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(entity.Damage.ArmorPenetration, Is.EqualTo(10));
        }
    }
}
