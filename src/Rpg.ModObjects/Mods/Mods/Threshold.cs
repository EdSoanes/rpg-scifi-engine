using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Threshold : Mod
    {
        [JsonConstructor] protected Threshold() { }

        public Threshold(string entityId, string prop, int min, int max)
            : base()
        {
            EntityId = entityId;
            Prop = prop;
            SourceValue = Dice.Zero;
            Behavior = new Behaviors.Threshold(min, max);
        }
    }
}
