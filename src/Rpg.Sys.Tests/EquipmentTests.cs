using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Tests
{
    public class EquipmentTests
    {
        private TestEquipment _equipment;
        private Graph _graph;
        private Actor _actor;

        [SetUp]
        public void Setup()
        {
            _graph = new Graph();
            _equipment = new TestEquipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            _actor = new Actor(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                }
            });

            _graph.Initialize(_actor);
            _graph.Entities.Add(_equipment);
        }

        [Test]
        public void CreateEquipment_VerifyTemplateName()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_equipment.Name, Is.EqualTo("Thing"));
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
            graph.Initialize(item);

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
