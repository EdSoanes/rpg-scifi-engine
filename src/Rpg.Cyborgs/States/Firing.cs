using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Firing : State<RangedWeapon>
    {
        [JsonConstructor] private Firing() { }

        public Firing(RangedWeapon owner)
            : base(owner) { }
    }
}
