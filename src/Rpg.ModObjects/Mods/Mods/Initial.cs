using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Initial : Mod
    {
        [JsonConstructor] protected Initial()
            : base(nameof(Initial))
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
