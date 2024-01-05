using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Archetypes.Templates;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class GunTests
    { 
        [TestMethod]
        public void Gun_Verify_Mods()
        {
            var gun = new Gun(new Rifle());
            var graph = new EntityGraph();
            graph.Initialize(gun);

            Assert.IsNotNull(graph.Context);
            Assert.IsNotNull(gun);

            Assert.AreEqual<string>("1d6", gun.Damage.Impact);
        }
    }
}
