using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TimedModSet : ModSet
    {
        [JsonConstructor] protected TimedModSet()
            : base()
        { }

        public TimedModSet(string ownerId, string name, Lifespan lifespan)
            : base(ownerId, name)
        {
            Lifespan = lifespan;
        }
    }
}
