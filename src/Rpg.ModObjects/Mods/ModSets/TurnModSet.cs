using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TurnModSet : TimedModSet
    {
        [JsonConstructor] protected TurnModSet()
            : base()
        { }

        public TurnModSet(string ownerId, string name)
            : base(ownerId, name, new Lifespan(0, 1))
        { }
    }
}
