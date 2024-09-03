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
            Target = new Props.PropRef(entityId, prop);
            SourceValue = Dice.Zero;
            Behavior = new Behaviors.Threshold(min, max);
        }
    }
}
