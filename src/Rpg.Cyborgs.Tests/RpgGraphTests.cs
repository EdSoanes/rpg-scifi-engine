using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;

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
            var json = RpgSerializer.Serialize(graph.GetGraphState());

            var graphState2 = RpgSerializer.Deserialize<RpgGraphState>(json);
            var graph2 = new RpgGraph(graphState2);

            var pc2 = graph2.Context as PlayerCharacter;

            Assert.That(pc2, Is.Not.Null);
            Assert.That(graph2.Time.Now, Is.EqualTo(graph.Time.Now));
            Assert.That(graph2.GetObjects().Count(), Is.EqualTo(graph.GetObjects().Count()));
            Assert.That(graph2.GetActiveMods().Count(), Is.EqualTo(graph.GetActiveMods().Count()));
            Assert.That(graph2.GetModSets().Count(), Is.EqualTo(graph.GetModSets().Count()));
        }
    }
}