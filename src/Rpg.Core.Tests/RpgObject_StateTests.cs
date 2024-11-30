using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.Core.Tests
{
    public class RpgObject_StateTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestPerson_State_IsHurt_Toggles()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.IsStateOn(nameof(Hurt)), Is.False);

            person.AddMod(new Permanent(), x => x.HitPoints, -1);
            graph.Time.TriggerEvent();

            Assert.That(person.IsStateOn(nameof(Hurt)), Is.True);

            person.AddMod(new Permanent(), x => x.HitPoints, 1);
            graph.Time.TriggerEvent();

            Assert.That(person.IsStateOn(nameof(Hurt)), Is.False);
        }

        [Test]
        public void ManuallyActivate_WeaponDamaged_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            weapon.SetStateOn(nameof(WeaponDamaged));
            graph.Time.TriggerEvent();

            Assert.That(weapon.IsStateOn(nameof(WeaponDamaged)), Is.True);
            Assert.That(weapon.HitBonus, Is.EqualTo(0));

            weapon.SetStateOff(nameof(WeaponDamaged));
            graph.Time.TriggerEvent();

            Assert.That(weapon.IsStateOn(nameof(WeaponDamaged)), Is.False);
            Assert.That(weapon.HitBonus, Is.EqualTo(1));
        }
    }
}
