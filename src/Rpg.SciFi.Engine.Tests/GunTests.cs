using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Archetypes;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class GunTests
    { 
        [TestMethod]
        public void Gun_Verify_Mods()
        {
            var gun = new Gun(10, 10);
            var mgr = new EntityManager<Gun>();
            mgr.Initialize(gun);

            Assert.IsNotNull(mgr.Context);
            Assert.IsNotNull(gun);

            Assert.AreEqual<string>("1d6", gun.Damage.BaseImpact);
            Assert.AreEqual<string>("1d6", gun.Damage.Impact);
        }
    }
}
