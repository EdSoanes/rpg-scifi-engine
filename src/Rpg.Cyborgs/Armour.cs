using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Newtonsoft.Json;

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
            this.AddMod(new Base(), x => x.CurrentArmourRating, x => x.ArmourRating);
        }
    }
}
