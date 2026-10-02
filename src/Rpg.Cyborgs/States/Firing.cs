using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Firing : RpgState<RangedWeapon>
    {
        [JsonConstructor] private Firing() { IsPlayerVisible = false; }

        public Firing(RangedWeapon owner)
            : base(owner) { IsPlayerVisible = false; }
    }
}
