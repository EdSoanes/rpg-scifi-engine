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

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(1));
            Assert.That(obj.Strength, Is.EqualTo(10));

            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void ModSet_StrengthMod_ExpireModSet()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            var objCount = characterSheet.GetObjectCount();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            modSet.Expire(characterSheet);
            characterSheet.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Destroyed));
            Assert.That(characterSheet.GetObjectCount(), Is.EqualTo(objCount - 1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(1));
            Assert.That(obj.Strength, Is.EqualTo(10));
        }

        [Test]
        public void ModSet_StrengthMod_UnapplyModSet()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 1);
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            modSet.Unapply();
            characterSheet.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(10));

            modSet.Apply();
            characterSheet.Time.Refresh();

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));
        }

        [Test]
        public void ModSet_StrengthMod_OneTurn()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var characterSheet = new RpgCharacterSheet(obj);
            var objData = characterSheet.GetObjectData(obj.Id);

            characterSheet.Time.BeginEncounter();

            var modSet = new RpgModSet().Lifespan(1);
            modSet.Add(obj, x => x.Strength, 1);
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            var objCount = characterSheet.GetObjectCount();

            Assert.That(modSet.Mods.Count, Is.EqualTo(1));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(11));

            characterSheet.Time.ToTurn(2);

            Assert.That(modSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(characterSheet.GetObjectCount(), Is.EqualTo(objCount));
            Assert.That(objData?.GetPropData<RpgPropertyDataModdable>("Strength")?.Mods.Count, Is.EqualTo(2));
            Assert.That(obj.Strength, Is.EqualTo(10));
        }
    }
}