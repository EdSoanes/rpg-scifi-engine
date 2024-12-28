using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public abstract class RpgObject : Lifespan, ILifecycle
    {
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public string Name { get; protected set; }

        public RpgObject()
            : base()
        {
            Archetype = GetType().Name;
        }

        public RpgObject(string name)
            : this()
        {
            Name = name;
        }
    }
}
