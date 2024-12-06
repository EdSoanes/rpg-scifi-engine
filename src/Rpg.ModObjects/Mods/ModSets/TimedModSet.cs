using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TimedModSet : ModSet
    {
        [JsonConstructor] protected TimedModSet()
            : base()
        { }

        public TimedModSet(string name, Lifespan lifespan)
            : base(null, name)
        {
            Lifespan = lifespan;
        }
    }
}
