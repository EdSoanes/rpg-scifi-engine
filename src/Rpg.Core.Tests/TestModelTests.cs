using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.Core.Tests
{
    public class TestModelTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestPerson_Stats_EnsureInitializeValues()
        {
            var person = new TestPerson("Benny");

            Assert.That(person.Strength, Is.EqualTo(13));
            Assert.That(person.StrengthBonus, Is.EqualTo(0));
            Assert.That(person.MeleeAttack, Is.EqualTo(1));
            Assert.That(person.MeleeDefence, Is.EqualTo(10));
            Assert.That(person.HitPoints, Is.EqualTo(10));

            var graph = new RpgGraph(person);

            Assert.That(person.Strength, Is.EqualTo(13));
            Assert.That(person.InitialValue(x => x.Strength)?.Roll() ?? 0, Is.EqualTo(13));

            Assert.That(person.StrengthBonus, Is.EqualTo(1));
            Assert.That(person.InitialValue(x => x.StrengthBonus)?.Roll() ?? 0, Is.EqualTo(0));
            Assert.That(person.BaseValue(x => x.StrengthBonus)?.Roll() ?? 0, Is.EqualTo(1));

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.InitialValue(x => x.MeleeAttack)?.Roll() ?? 0, Is.EqualTo(1));

            Assert.That(person.MeleeDefence, Is.EqualTo(10));
            Assert.That(person.InitialValue(x => x.MeleeDefence)?.Roll() ?? 0, Is.EqualTo(10));

            Assert.That(person.HitPoints, Is.EqualTo(10));
            Assert.That(person.InitialValue(x => x.HitPoints)?.Roll() ?? 0, Is.EqualTo(10));
        }

        [Test]
        public void TestPerson_Collections_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var weapon2 = new TestWeapon("Sword2");
            person.Wearing.Add(weapon2);

            var graph = new RpgGraph(person);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(1));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.True);
        }

        [Test]
        public void TestPerson_States_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.States.Count(), Is.EqualTo(2));
            Assert.That(person.GetState(nameof(Hurt)), Is.Not.Null);
            Assert.That(person.GetState(nameof(Attacking)), Is.Not.Null);
        }

        [Test]
        public void TestWeapon_States_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            Assert.That(weapon.States.Count(), Is.EqualTo(1));
            Assert.That(weapon.GetState(nameof(WeaponDamaged)), Is.Not.Null);
        }

        [Test]
        public void TestWeapon_Actions_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            Assert.That(weapon.ActionTemplates.Count(), Is.EqualTo(1));
            Assert.That(weapon.ActionTemplates[nameof(Attack)], Is.Not.Null);
        }
    }
}
