using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Initial : Mod
    {
        [JsonConstructor] protected Initial()
            : base()
        {
            Behavior = new Replace();
        }

        public Initial(string entityId, string prop, Dice sourceValue)
            : this()
        {
            Target = new Props.PropRef(entityId, prop);
            SourceValue = sourceValue;
        }
    }
}
