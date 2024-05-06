using Newtonsoft.Json;
using Rpg.Sys.Moddable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class DamageValue : ModObject
    {
        [JsonProperty] public Dice Dice { get; protected set; }
        [JsonProperty] public int ArmorPenetration { get; protected set; }
        [JsonProperty] public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue (Dice dice, int armorPenetration, int radius)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
