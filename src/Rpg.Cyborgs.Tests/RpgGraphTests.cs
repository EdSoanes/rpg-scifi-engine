using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;

namespace Rpg.Cyborgs.Tests
{
    public class RpgGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void RpgGraph_BennyHasSword_EnsureSerialization()
        {
            var sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            pc.Hands.Add(sword);

            var graph = new RpgGraph(pc);
            var json = graph.Serialize();

            var graph2 = RpgGraph.Deserialize(json);
            var pc2 = graph2.Context as PlayerCharacter;

            Assert.That(pc2, Is.Not.Null);
            Assert.That(graph2.Time.Current, Is.EqualTo(graph.Time.Current));
            Assert.That(graph2.GetEntities().Count(), Is.EqualTo(graph.GetEntities().Count()));
            Assert.That(graph2.GetActiveMods().Count(), Is.EqualTo(graph.GetActiveMods().Count()));
            Assert.That(graph2.GetModSets().Count(), Is.EqualTo(graph.GetModSets().Count()));
        }
    }
}