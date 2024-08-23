using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TimedModSet : ModSet
    {
        [JsonConstructor] protected TimedModSet()
            : base()
        { }

        public TimedModSet(string ownerId, string name, SpanOfTime lifespan)
            : base(ownerId, name)
        {
            Lifespan = lifespan;
        }
    }
}
