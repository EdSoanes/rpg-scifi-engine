using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Firing : State<RangedWeapon>
    {
        [JsonConstructor] private Firing() { }

        public Firing(RangedWeapon owner)
            : base(owner) { }
    }
}
