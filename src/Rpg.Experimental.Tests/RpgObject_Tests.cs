using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Tests
{

    public class RpgObject_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void PropertyValues_EnsureObjectData()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            graph.Time.Refresh();

            Assert.That(graph.Objects.Count, Is.EqualTo(6));
            Assert.That(graph.Objects.ContainsKey(obj.Id), Is.True);

            Assert.That(graph.ObjectData.Count, Is.EqualTo(2));
            Assert.That(graph.ObjectData.ContainsKey(obj.Id), Is.True);
            Assert.That(graph.ObjectData[obj.Id].Props.Count, Is.EqualTo(9));
            Assert.That(graph.ObjectData[obj.Child.Id].Props.Count, Is.EqualTo(9));

            var strength = graph.GetPropertyData(obj.Id, "Strength") as RpgPropertyDataModdable;
            Assert.That(strength, Is.Not.Null);
            Assert.That(strength.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(strength.IsNullable, Is.False);

            var intelligence = graph.GetPropertyData(obj.Id, "Intelligence") as RpgPropertyDataModdable;
            Assert.That(intelligence, Is.Not.Null);
            Assert.That(intelligence.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(intelligence.IsNullable, Is.True);

            var damage = graph.GetPropertyData(obj.Id, "Damage") as RpgPropertyDataModdable;
            Assert.That(damage, Is.Not.Null);
            Assert.That(damage.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(damage.IsNullable, Is.False);

            var initiative = graph.GetPropertyData(obj.Id, "Initiative") as RpgPropertyDataModdable;
            Assert.That(initiative, Is.Not.Null);
            Assert.That(initiative.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(initiative.IsNullable, Is.True);

            var child = graph.GetPropertyData(obj.Id, "Child") as RpgPropertyDataObject;
            Assert.That(child, Is.Not.Null);
            Assert.That(child.IsNullable, Is.True);
            Assert.That(child.Refs.Count, Is.EqualTo(1));
            Assert.That(child.Refs.First().ChildObjectId, Is.EqualTo(obj.Child.Id));
        }

        [Test]
        public void RpgObject_AddStrengthMod_EnsurePropValue()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            graph.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));
            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void RpgObject_AddStrengthMod_Replace_EnsurePropValue()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var strPropData = graph.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            Assert.That(strPropData, Is.Not.Null);
            Assert.That(obj.Strength, Is.EqualTo(10));

            graph.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var replaceMods = strPropData.Mods.Where(x => x is Replace);
            var replaceMod = replaceMods.FirstOrDefault();

            Assert.That(replaceMods.Count(), Is.EqualTo(1));
            Assert.That(replaceMod, Is.Not.Null);
            Assert.That(replaceMod.Source?.Value, Is.EqualTo(new Dice(1)));

            graph.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(2));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            replaceMods = strPropData.Mods.Where(x => x is Replace);
            replaceMod = strPropData.Mods.FirstOrDefault(x => x is Replace);
            Assert.That(replaceMods.Count(), Is.EqualTo(1));
            Assert.That(replaceMod?.Version, Is.EqualTo(0));
            Assert.That(replaceMod?.Source?.Value, Is.EqualTo(new Dice(2)));
        }

        [Test]
        public void RpgObject_AddStrengthMod_Replace_EndEncounter()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var strPropData = graph.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            graph.Time.BeginEncounter();

            Assert.That(strPropData, Is.Not.Null);
            Assert.That(obj.Strength, Is.EqualTo(10));

            graph.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var replaceMods = strPropData.Mods.Where(x => x is Replace);
            var replaceMod = replaceMods.FirstOrDefault();

            Assert.That(replaceMods.Count(), Is.EqualTo(1));
            Assert.That(replaceMod, Is.Not.Null);
            Assert.That(replaceMod.Source?.Value, Is.EqualTo(new Dice(1)));

            graph.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(2));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(3));

            replaceMods = strPropData.Mods.Where(x => x is Replace);
            replaceMod = strPropData.Mods.FirstOrDefault(x => x is Replace);
            Assert.That(replaceMods.Count(), Is.EqualTo(2));
            Assert.That(replaceMods.Count(x => x.Version == 0), Is.EqualTo(1));
            Assert.That(replaceMods.Count(x => x.Version == 1), Is.EqualTo(1));
        }

        [Test]
        public void RpgObject_AddStrengthMod_Combine_EnsurePropValue()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);
            var strPropData = graph.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            Assert.That(obj.Strength, Is.EqualTo(10));
            graph.Add(new Combine()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData, Is.Not.Null);
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var combineMods = strPropData.Mods.Where(x => x is Combine);
            var combineMod = combineMods.FirstOrDefault();

            Assert.That(combineMods.Count(), Is.EqualTo(1));
            Assert.That(combineMod, Is.Not.Null);
            Assert.That(combineMod.Source?.Value, Is.EqualTo(new Dice(1)));

            graph.Add(new Combine()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            graph.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));

            combineMods = strPropData.Mods.Where(x => x is Combine);

            Assert.That(combineMods.Count(), Is.EqualTo(1));
            Assert.That(combineMods.All(x => x.Source?.Value == new Dice(2)), Is.True);
        }

        [Test]
        public void RpgObject_AddStrengthMod_OneTurn_EnsurePropValue()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            graph.Time.BeginEncounter();
            graph.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1)
                .Lifespan(1));

            graph.Time.Refresh();
            Assert.That(obj.Strength, Is.EqualTo(11));

            graph.Time.ToTurn(2);
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void RpgObject_AddStrengthMod_TimePasses_EnsurePropValue()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            graph.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1)
                .Lifespan(TimePointType.Waiting, new TimePoint(TimePointType.TimePasses, 1)));

            graph.Time.Refresh();
            Assert.That(obj.Strength, Is.EqualTo(11));

            graph.Time.TimePasses();
            Assert.That(obj.Strength, Is.EqualTo(11));

            graph.Time.TimePasses(1);
            Assert.That(obj.Strength, Is.EqualTo(10));

        }

        [Test]
        public void RpgObject_AddStrengthMod_EnsureChangeTracker()
        {
            var obj = new TestObject();
            var graph = new RpgGraph(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            graph.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            Assert.That(graph.ChangeTracker.UpdatedProps.Count, Is.EqualTo(1));

            graph.Time.Refresh();

            Assert.That(graph.ChangeTracker.UpdatedProps.Count, Is.EqualTo(0));
        }

        [Test]
        public void RpgObject_MoveChildToChildren_EnsurePropValues()
        {
            var obj = new TestObject("ParentObj");
            var childObj = new TestObject("ChildObj");
            obj.Child = childObj;
            var graph = new RpgGraph(obj);

            Assert.That(obj.Child, Is.Not.Null);
            graph.Move(obj.Id, nameof(TestObject.Children), childObj.Id);
            graph.Time.Refresh();

            Assert.That(obj.Child, Is.Null);
            Assert.That(obj.Children, Is.Not.Null);
            Assert.That(obj.Children.Count, Is.EqualTo(1));
            Assert.That(obj.Children.First().Id, Is.EqualTo(childObj.Id));
        }
    }
}