using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using Rpg.Sys.Tests.Factories;

namespace Rpg.Sys.Tests
{
    public class EquipmentTests
    {
        private TestEquipment equipment;
        private Graph graph;
        private Actor human;

        [Test]
        public void CreateEquipment_IsA()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(equipment.Is.Count(), Is.EqualTo(4));
            Assert.That(equipment.Is, Contains.Item("ModdableObject"));
            Assert.That(equipment.Is, Contains.Item("Artifact"));
            Assert.That(equipment.Is, Contains.Item("Equipment"));
            Assert.That(equipment.Is, Contains.Item("TestEquipment"));
        }

        [Test]
        public void CreateEquipment_VerifyTemplateName()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(equipment, Is.Not.Null);
            Assert.That(equipment.Name, Is.EqualTo("Thing"));
        }

        [Test]
        public void CreateEquipment_VerifyPropertyValues()
        {
            var template = new ArtifactTemplate
            {
                Damage = new DamageTemplate
                {
                    Kinetic = "d6"
                },
                Presence = new PresenceTemplate
                {
                    SoundMax = 10
                },
                Defenses = new DefensesTemplate
                {
                    Energy = 5
                },
                Health = new HealthTemplate
                {
                    Physical = 6
                }
            };

            var graph = new Graph();
            var item = new TestEquipment(template);
            graph.SetContext(item);

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Damage.Kinetic.Dice, Is.EqualTo(new Dice("1d6")));
            Assert.That(item.Damage.Kinetic.ArmorPenetration, Is.EqualTo(0));
            Assert.That(item.Damage.Kinetic.Radius, Is.EqualTo(0));

            Assert.That(item.Presence.Sound.Max, Is.EqualTo(10));
            Assert.That(item.Presence.Sound.Current, Is.EqualTo(0));
            
            Assert.That(item.Defenses.Kinetic.Value, Is.EqualTo(0));
            Assert.That(item.Defenses.Kinetic.Shielding, Is.EqualTo(0));

            Assert.That(item.Defenses.Energy.Value, Is.EqualTo(5));
            Assert.That(item.Defenses.Energy.Shielding, Is.EqualTo(0));

            Assert.That(item.Health.Physical.Current, Is.EqualTo(6));
            Assert.That(item.Health.Physical.Max, Is.EqualTo(6));

            Assert.That(item.Health.Mental.Current, Is.EqualTo(0));
            Assert.That(item.Health.Mental.Max, Is.EqualTo(0));
        }
    }
}
