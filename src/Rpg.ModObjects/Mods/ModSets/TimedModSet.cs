using Rpg.ModObjects.Time;
using System.Text.Json.Serialization;

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
