using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Turn : Time
    {
        public Turn()
            : base(0, 1)
        { }

        public Turn(int duration)
            : base(0, duration)
        { }

        public Turn(int startTurn, int duration)
            : base(startTurn, duration)
        { }
    }
}
