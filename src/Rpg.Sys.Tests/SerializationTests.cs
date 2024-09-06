using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Tests
{
    public class SerializationTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(MetaSystem).Assembly);
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
            
            var json = RpgJson.Serialize(human);
            Assert.That(json, Is.Not.Null);

            var human2 = RpgJson.Deserialize<Human>(json);
            Assert.That(human2, Is.Not.Null);
        }
    }
}