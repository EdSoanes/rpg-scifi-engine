using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class TurnModSet : TimedModSet
    {
        [JsonConstructor] protected TurnModSet()
            : base()
        { }

        public TurnModSet(string name)
            : base(name, new Lifespan(0, 1))
        { }
    }
}
