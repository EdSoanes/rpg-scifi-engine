using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs
{
    public class Armour : RpgEntity
    {
        [JsonProperty]
        public int ArmourRating { get; protected set; }

        [JsonProperty]
        public int CurrentArmourRating { get; protected set; }

        [JsonProperty]
        public int DefenceModifier { get; protected set; }

        public override void OnTimeBegins()
        {
            this.BaseMod(x => x.CurrentArmourRating, x => x.ArmourRating);
        }
    }
}
