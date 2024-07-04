using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class GunFiring : State<TestGun>
    {
        [JsonConstructor] private GunFiring() { }

        public GunFiring(TestGun owner)
            : base(owner)
        { }
    }
}
