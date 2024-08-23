using Newtonsoft.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TurnModSet : TimedModSet
    {
        [JsonConstructor] protected TurnModSet()
            : base()
        { }

        public TurnModSet(string ownerId, string name)
            : base(ownerId, name, new SpanOfTime(0, 1))
        { }
    }
}
