using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Tests
{
    public class SerializationTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(MetaSystem).Assembly);
        }

        [Test]
        public void CreateHuman_Serialize_EnsureValues()
        {
            var human = new Human(new ActorTemplate
            {
                Stats = new Components.StatPointsTemplate
                {
                    Strength = 13,
                    Intelligence = 14,
                    Wisdom = 8,
                    Dexterity= 5,
                    Constitution = 6,
                    Charisma = 18
                }
            });
            
            var json = RpgSerializer.Serialize(human);
            Assert.That(json, Is.Not.Null);

            var human2 = RpgSerializer.Deserialize<Human>(json);
            Assert.That(human2, Is.Not.Null);
        }
    }
}