using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class DamageValue : ModdableObject
    {
        [JsonProperty] public Dice Dice { get; private set; }
        [JsonProperty] public int ArmorPenetration { get; private set; }
        [JsonProperty] public int Radius { get; private set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue (Dice dice, int armorPenetration, int radius)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
