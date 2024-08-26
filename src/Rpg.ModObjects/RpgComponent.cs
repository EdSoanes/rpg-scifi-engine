using Newtonsoft.Json;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects
{
    public abstract class RpgComponent : RpgObject
    {
        [JsonConstructor] protected RpgComponent() { }

        public RpgComponent(string name)
        {
            Name = name;
        }
    }
}
