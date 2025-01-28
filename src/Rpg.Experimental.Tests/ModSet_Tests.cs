using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{
    public class ModSet_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void ModSet_StrengthMod_EnsureObjectData()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            var objData = graph.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(1));
            Assert.That(obj.Strength, Is.EqualTo(10));

            graph.Add(modSet);
            graph.Time.Refresh();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void ModSet_StrengthMod_ExpireModSet()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            var objData = graph.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);
            graph.Add(modSet);
            graph.Time.Refresh();

            var objCount = graph.Objects.Count;

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            modSet.Expire(graph);
            graph.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
            Assert.That(graph.Objects.Count, Is.EqualTo(objCount - 1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(1));
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void ModSet_StrengthMod_UnapplyModSet()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            var objData = graph.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);
            graph.Add(modSet);
            graph.Time.Refresh();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            modSet.Unapply();
            graph.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(10));

            modSet.Apply();
            graph.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void ModSet_StrengthMod_OneTurn()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            var objData = graph.GetObjectData(obj.Id);

            graph.Time.BeginEncounter();

            var modSet = new RpgModSet().Lifespan(1);
            modSet.Add(obj, x => x.Strength, 1);
            graph.Add(modSet);
            graph.Time.Refresh();

            var objCount = graph.Objects.Count;

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            graph.Time.ToTurn(2);

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(graph.Objects.Count, Is.EqualTo(objCount));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(10));
        }
    }
}