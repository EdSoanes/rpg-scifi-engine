using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Mods
{
    public class Temporal : Mod
    {
        [JsonConstructor] protected Temporal()
            : base()
        { }

        public Temporal(int duration)
            : base()
                => Lifespan(duration);

        public Temporal(int startsIn, int duration)
            : base()
                => Lifespan(startsIn, duration);

        public Temporal(TimePoint start, TimePoint end)
            : base()
                => Lifespan(start, end);
    }
}
