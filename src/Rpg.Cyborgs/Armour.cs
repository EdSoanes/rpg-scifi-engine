using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs
{
    public class Armour : RpgEntity
    {
        [JsonInclude]
        public int ArmourRating { get; protected set; }

        [JsonInclude]
        public int CurrentArmourRating { get; protected set; }

        [JsonInclude]
        public int DefenceModifier { get; protected set; }

        public override void OnTimeBegins()
        {
            this.AddMod(new Base(), x => x.CurrentArmourRating, x => x.ArmourRating);
        }
    }
}
