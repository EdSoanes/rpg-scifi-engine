using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class Firing : State<RangedWeapon>
    {
        [JsonConstructor] private Firing() { }

        public Firing(RangedWeapon owner)
            : base(owner) { }
    }
}
