using Rpg.Experimental.Graph;
using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;
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

            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.GetObjectCount(), Is.EqualTo(6));
            Assert.That(characterSheet.ObjectExists(obj.Id), Is.True);

            Assert.That(characterSheet.GetObjectDataCount(), Is.EqualTo(2));
            Assert.That(characterSheet.ObjectDataExists(obj.Id), Is.True);
            Assert.That(characterSheet.GetObjectData(obj.Id)!.Props.Count, Is.EqualTo(9));
            Assert.That(characterSheet.GetObjectData(obj.Child.Id)!.Props.Count, Is.EqualTo(9));

            var strength = characterSheet.GetPropertyData(obj.Id, "Strength") as RpgPropertyDataModdable;
            Assert.That(strength, Is.Not.Null);
            Assert.That(strength.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(strength.IsNullable, Is.False);

            var intelligence = characterSheet.GetPropertyData(obj.Id, "Intelligence") as RpgPropertyDataModdable;
            Assert.That(intelligence, Is.Not.Null);
            Assert.That(intelligence.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(intelligence.IsNullable, Is.True);

            var damage = characterSheet.GetPropertyData(obj.Id, "Damage") as RpgPropertyDataModdable;
            Assert.That(damage, Is.Not.Null);
            Assert.That(damage.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(damage.IsNullable, Is.False);

            var initiative = characterSheet.GetPropertyData(obj.Id, "Initiative") as RpgPropertyDataModdable;
            Assert.That(initiative, Is.Not.Null);
            Assert.That(initiative.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(initiative.IsNullable, Is.True);

            var child = characterSheet.GetPropertyData(obj.Id, "Child") as RpgPropertyDataObject;
            Assert.That(child, Is.Not.Null);
            Assert.That(child.IsNullable, Is.True);
            Assert.That(child.Refs.Count, Is.EqualTo(1));
            Assert.That(child.Refs.First().ChildObjectId, Is.EqualTo(obj.Child.Id));
        }

        [Test]
        public void PropertyValues_TestObject_Serialize_EnsureObjectData()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var json = RpgJson.Serialize(obj);
            var obj2 = RpgJson.Deserialize<TestObject>(json);
        }

        [Test]
        public void PropertyValues_GraphState_Serialize_EnsureObjectData()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.Refresh();

            var characterSheetState = characterSheet.GetState();
            var system = characterSheet.GetSystem();

            var characterSheetJson = RpgJson.SerializeGraphState(characterSheetState);
            var systemJson = RpgJson.Serialize(system);

            var characterSheetJson2 = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(characterSheetJson);
            var system2 = RpgJson.Deserialize<RpgSystem>(systemJson);

            var characterSheet2 = new RpgCharacterSheet(characterSheetJson2, system2);

            Assert.That(characterSheet2.GetObjectCount(), Is.EqualTo(6));
            Assert.That(characterSheet2.ObjectExists(obj.Id), Is.True);

            Assert.That(characterSheet2.GetObjectDataCount(), Is.EqualTo(2));
            Assert.That(characterSheet2.ObjectDataExists(obj.Id), Is.True);
            Assert.That(characterSheet2.GetObjectData(obj.Id)!.Props.Count, Is.EqualTo(9));
            Assert.That(characterSheet2.GetObjectData(obj.Child.Id)!.Props.Count, Is.EqualTo(9));

            var strength = characterSheet2.GetPropertyData(obj.Id, "Strength") as RpgPropertyDataModdable;
            Assert.That(strength, Is.Not.Null);
            Assert.That(strength.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(strength.IsNullable, Is.False);

            var intelligence = characterSheet2.GetPropertyData(obj.Id, "Intelligence") as RpgPropertyDataModdable;
            Assert.That(intelligence, Is.Not.Null);
            Assert.That(intelligence.PropType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(intelligence.IsNullable, Is.True);

            var damage = characterSheet2.GetPropertyData(obj.Id, "Damage") as RpgPropertyDataModdable;
            Assert.That(damage, Is.Not.Null);
            Assert.That(damage.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(damage.IsNullable, Is.False);

            var initiative = characterSheet2.GetPropertyData(obj.Id, "Initiative") as RpgPropertyDataModdable;
            Assert.That(initiative, Is.Not.Null);
            Assert.That(initiative.PropType, Is.EqualTo(RpgPropertyType.Dice));
            Assert.That(initiative.IsNullable, Is.True);

            var child = characterSheet2.GetPropertyData(obj.Id, "Child") as RpgPropertyDataObject;
            Assert.That(child, Is.Not.Null);
            Assert.That(child.IsNullable, Is.True);
            Assert.That(child.Refs.Count, Is.EqualTo(1));
            Assert.That(child.Refs.First().ChildObjectId, Is.EqualTo(obj.Child.Id));
        }

        [Test]
        public void RpgObject_AddStrengthMod_EnsurePropValue()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            characterSheet.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void RpgObject_AddStrengthMod_Replace_EnsurePropValue()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            var strPropData = characterSheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            Assert.That(strPropData, Is.Not.Null);
            Assert.That(obj.Strength, Is.EqualTo(10));

            characterSheet.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var replaceMods = strPropData.Mods.Where(x => x is Replace);
            var replaceMod = replaceMods.FirstOrDefault();

            Assert.That(replaceMods.Count(), Is.EqualTo(1));
            Assert.That(replaceMod, Is.Not.Null);
            Assert.That(replaceMod.Source?.Value, Is.EqualTo(new Dice(1)));

            characterSheet.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(2));

            characterSheet.Time.Refresh();

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
            var characterSheet = new RpgCharacterSheet(obj);
            var strPropData = characterSheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            characterSheet.Time.BeginEncounter();

            Assert.That(strPropData, Is.Not.Null);
            Assert.That(obj.Strength, Is.EqualTo(10));

            characterSheet.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var replaceMods = strPropData.Mods.Where(x => x is Replace);
            var replaceMod = replaceMods.FirstOrDefault();

            Assert.That(replaceMods.Count(), Is.EqualTo(1));
            Assert.That(replaceMod, Is.Not.Null);
            Assert.That(replaceMod.Source?.Value, Is.EqualTo(new Dice(1)));

            characterSheet.Add(new Replace()
                .SetTarget(obj, x => x.Strength)
                .SetSource(2));

            characterSheet.Time.Refresh();

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
            var characterSheet = new RpgCharacterSheet(obj);
            var strPropData = characterSheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "Strength");

            Assert.That(obj.Strength, Is.EqualTo(10));
            characterSheet.Add(new Combine()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(11));
            Assert.That(strPropData, Is.Not.Null);
            Assert.That(strPropData.Mods.Count, Is.EqualTo(2));

            var combineMods = strPropData.Mods.Where(x => x is Combine);
            var combineMod = combineMods.FirstOrDefault();

            Assert.That(combineMods.Count(), Is.EqualTo(1));
            Assert.That(combineMod, Is.Not.Null);
            Assert.That(combineMod.Source?.Value, Is.EqualTo(new Dice(1)));

            characterSheet.Add(new Combine()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(12));

            combineMods = strPropData.Mods.Where(x => x is Combine);

            Assert.That(combineMods.Count(), Is.EqualTo(1));
            Assert.That(combineMods.All(x => x.Source?.Value == new Dice(2)), Is.True);
        }

        [Test]
        public void RpgObject_AddStrengthMod_OneTurn_EnsurePropValue()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            characterSheet.Time.BeginEncounter();
            characterSheet.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1)
                .Lifespan(1));

            characterSheet.Time.Refresh();
            Assert.That(obj.Strength, Is.EqualTo(11));

            characterSheet.Time.ToTurn(2);
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void RpgObject_AddStrengthMod_TimePasses_EnsurePropValue()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            characterSheet.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1)
                .Lifespan(TimePointType.Waiting, new TimePoint(TimePointType.TimePasses, 1)));

            characterSheet.Time.Refresh();
            Assert.That(obj.Strength, Is.EqualTo(11));

            characterSheet.Time.TimePasses();
            Assert.That(obj.Strength, Is.EqualTo(11));

            characterSheet.Time.TimePasses(1);
            Assert.That(obj.Strength, Is.EqualTo(10));

        }

        [Test]
        public void RpgObject_AddStrengthMod_EnsureChangeTracker()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            characterSheet.Add(new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(1));

            Assert.That(characterSheet.GetChangedProperties().Count(), Is.EqualTo(1));

            characterSheet.Time.Refresh();

            Assert.That(characterSheet.GetChangedProperties().Count(), Is.EqualTo(0));
        }

        [Test]
        public void RpgObject_MoveChildToChildren_EnsurePropValues()
        {
            var obj = new TestObject("ParentObj");
            var childObj = new TestObject("ChildObj");
            obj.Child = childObj;
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Child, Is.Not.Null);
            characterSheet.Move(obj.Id, nameof(TestObject.Children), childObj.Id);
            characterSheet.Time.Refresh();

            Assert.That(obj.Child, Is.Null);
            Assert.That(obj.Children, Is.Not.Null);
            Assert.That(obj.Children.Count, Is.EqualTo(1));
            Assert.That(obj.Children.First().Id, Is.EqualTo(childObj.Id));
        }
    }
}