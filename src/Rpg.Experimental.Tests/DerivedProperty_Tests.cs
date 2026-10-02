using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// When a property changes, the properties derived from it must be updated on their objects too
    /// </summary>
    public class DerivedProperty_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        [Test]
        public void SourceChanged_DerivedProps_Updated()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));
            Assert.That(obj.Damage.ToString(), Is.EqualTo("1d6 + 11"));

            characterSheet.Add(obj, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));
            Assert.That(obj.StrengthBonus, Is.EqualTo(2));
            Assert.That(obj.Damage.ToString(), Is.EqualTo("1d6 + 15"));

            //Unrelated props are left alone
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(obj.IntelligenceBonus, Is.EqualTo(-4));
        }

        [Test]
        public void SourceChanged_ModExpired_DerivedProps_Reverted()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            var mod = new Standard()
                .SetTarget(obj, x => x.Strength)
                .SetSource(4);

            characterSheet.Add(mod);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));
            Assert.That(obj.StrengthBonus, Is.EqualTo(2));

            characterSheet.Expire(mod);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));
            Assert.That(obj.Damage.ToString(), Is.EqualTo("1d6 + 11"));
        }

        [Test]
        public void SourceChanged_ModSetUnapplied_DerivedProps_Reverted()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            var modSet = new RpgModSet();
            modSet.Add(obj, x => x.Strength, 4);
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));
            Assert.That(obj.StrengthBonus, Is.EqualTo(2));

            modSet.Unapply();
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));

            modSet.Apply();
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));
            Assert.That(obj.StrengthBonus, Is.EqualTo(2));
        }

        [Test]
        public void SourceChanged_ChainAcrossObjects_DerivedProps_Updated()
        {
            var parent = new TestObject("Parent");
            var child = new TestObject("Child");
            parent.Child = child;

            var characterSheet = new RpgCharacterSheet(parent);

            //parent.Strength => parent.StrengthBonus => child.Strength => child.StrengthBonus and child.Damage
            characterSheet.Add(new Base(), child, x => x.Strength, parent, x => x.StrengthBonus);
            characterSheet.Time.Refresh();

            Assert.That(parent.StrengthBonus, Is.EqualTo(0));
            Assert.That(child.Strength, Is.EqualTo(10));
            Assert.That(child.StrengthBonus, Is.EqualTo(0));

            characterSheet.Add(parent, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(parent.Strength, Is.EqualTo(14));
            Assert.That(parent.StrengthBonus, Is.EqualTo(2));
            Assert.That(child.Strength, Is.EqualTo(12));
            Assert.That(child.StrengthBonus, Is.EqualTo(1));
            Assert.That(child.Damage.ToString(), Is.EqualTo("1d6 + 13"));
        }

        [Test]
        public void SourceChanged_DuringEncounter_DerivedProps_FollowModLifespan()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Time.BeginEncounter();

            characterSheet.Add(new Temporal(1), obj, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(14));
            Assert.That(obj.StrengthBonus, Is.EqualTo(2));

            characterSheet.Time.ToTurn(2);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));
        }

        [Test]
        public void SourceChanged_DerivedStateCondition_Reacts()
        {
            var obj = new TestObject();
            var characterSheet = new RpgCharacterSheet(obj);

            //Intelligence is derived from Strength, the test state is on when Intelligence > 3
            characterSheet.Add(new Standard(), obj, x => x.Intelligence, x => x.StrengthBonus);
            characterSheet.Time.Refresh();

            var testState = characterSheet.GetObjectState(obj.Id, nameof(TestState))!;
            Assert.That(obj.Intelligence, Is.EqualTo(3));
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Suspended));
            Assert.That(obj.Initiative, Is.Null);

            characterSheet.Add(obj, x => x.Strength, 4);
            characterSheet.Time.Refresh();

            Assert.That(obj.StrengthBonus, Is.EqualTo(2));
            Assert.That(obj.Intelligence, Is.EqualTo(5));
            Assert.That(obj.IntelligenceBonus, Is.EqualTo(-3));
            Assert.That(testState.Expiry, Is.EqualTo(LifecycleExpiry.Active));
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(1)));
        }
    }
}
