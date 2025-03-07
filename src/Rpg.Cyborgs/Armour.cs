using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs
{
    public class Armour : RpgObject
    {
        [JsonProperty]
        public int ArmourRating { get; protected set; }

        [JsonProperty]
        public int CurrentArmourRating { get; protected set; }

        [JsonProperty]
        public int DefenceModifier { get; protected set; }

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            base.OnCreating(graph, owner);
            graph
                .Add(new Base(), this, x => x.CurrentArmourRating, x => x.ArmourRating);
        }
    }
}
