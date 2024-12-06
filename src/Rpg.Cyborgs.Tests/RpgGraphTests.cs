using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;

namespace Rpg.Cyborgs.Tests
{
    public class RpgGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void RpgGraph_BennyHasSword_EnsureSerialization()
        {
            var sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            pc.Hands.Add(sword);

            var graph = new RpgGraph(pc);
            var json = RpgJson.Serialize(graph.GetGraphState());

            var graphState2 = RpgJson.Deserialize<RpgGraphState>(json);
            var graph2 = new RpgGraph(graphState2);

            var pc2 = graph2.Context as PlayerCharacter;

            Assert.That(pc2, Is.Not.Null);
            Assert.That(graph2.Time.Now, Is.EqualTo(graph.Time.Now));
            Assert.That(graph2.GetObjects().Count(), Is.EqualTo(graph.GetObjects().Count()));
            Assert.That(graph2.GetModSets().Count(), Is.EqualTo(graph.GetModSets().Count()));

            foreach (var rpgObj in graph.GetObjects())
            {
                var rpgObj2 = graph2.GetObject(rpgObj.Id);
                Assert.That(rpgObj2, Is.Not.Null);
                Assert.That(rpgObj2.GetMods().Count(), Is.EqualTo(rpgObj.GetMods().Count()));
            }
        }
    }
}